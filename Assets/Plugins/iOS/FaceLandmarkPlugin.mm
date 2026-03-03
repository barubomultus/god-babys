#import <Foundation/Foundation.h>
#import <Vision/Vision.h>
#import <UIKit/UIKit.h>

// ─── Helper: Vision座標 (bottom-left原点) → top-left原点 ───

static CGPoint VisionToTopLeft(CGPoint visionPt) {
    return CGPointMake(visionPt.x, 1.0 - visionPt.y);
}

// faceBounds内の相対座標 → 画像全体の正規化座標 (top-left原点)
static CGPoint LandmarkToNormalized(CGPoint landmarkPt, CGRect faceBounds) {
    // landmarkPt は faceBounds 相対 (0-1, bottom-left原点)
    CGFloat imgX = faceBounds.origin.x + landmarkPt.x * faceBounds.size.width;
    CGFloat imgY = faceBounds.origin.y + landmarkPt.y * faceBounds.size.height;
    // Vision の Y は bottom-left → top-left に変換
    return CGPointMake(imgX, 1.0 - imgY);
}

// ─── Helper: ランドマーク領域の重心 ───

static CGPoint CentroidOfRegion(VNFaceLandmarkRegion2D *region, CGRect faceBounds) {
    if (region == nil || region.pointCount == 0) {
        return CGPointMake(-1, -1);
    }
    const CGPoint *points = region.normalizedPoints;
    CGFloat sumX = 0, sumY = 0;
    NSUInteger count = region.pointCount;
    for (NSUInteger i = 0; i < count; i++) {
        sumX += points[i].x;
        sumY += points[i].y;
    }
    CGPoint centroid = CGPointMake(sumX / count, sumY / count);
    return LandmarkToNormalized(centroid, faceBounds);
}

// ─── Helper: 頬位置を推定（目と口の中間、横にオフセット）───

static CGPoint EstimateCheek(CGPoint eye, CGPoint mouth, BOOL isLeft) {
    CGFloat midX = (eye.x + mouth.x) * 0.5;
    CGFloat midY = (eye.y + mouth.y) * 0.5;
    // 左頬は左にオフセット、右頬は右にオフセット
    CGFloat offset = (isLeft ? -0.06 : 0.06);
    return CGPointMake(midX + offset, midY);
}

// ─── メインAPI ───

extern "C" {

char* _FaceLandmark_DetectFromPNG(const unsigned char* pngData, int dataLength) {
    @autoreleasepool {
        if (pngData == NULL || dataLength <= 0) {
            return NULL;
        }

        NSData *data = [NSData dataWithBytes:pngData length:dataLength];
        UIImage *image = [UIImage imageWithData:data];
        if (image == nil) {
            NSLog(@"[FaceLandmark] Failed to create UIImage from PNG data");
            return NULL;
        }

        CIImage *ciImage = [[CIImage alloc] initWithImage:image];
        if (ciImage == nil) {
            NSLog(@"[FaceLandmark] Failed to create CIImage");
            return NULL;
        }

        // VNDetectFaceLandmarksRequest 実行
        __block VNFaceObservation *bestFace = nil;

        VNDetectFaceLandmarksRequest *request = [[VNDetectFaceLandmarksRequest alloc]
            initWithCompletionHandler:^(VNRequest *req, NSError *error) {
                if (error != nil) {
                    NSLog(@"[FaceLandmark] Detection error: %@", error.localizedDescription);
                    return;
                }
                NSArray<VNFaceObservation *> *faces = req.results;
                if (faces.count == 0) {
                    NSLog(@"[FaceLandmark] No faces detected");
                    return;
                }

                // 最大面積の顔を選択
                CGFloat maxArea = 0;
                for (VNFaceObservation *face in faces) {
                    CGFloat area = face.boundingBox.size.width * face.boundingBox.size.height;
                    if (area > maxArea) {
                        maxArea = area;
                        bestFace = face;
                    }
                }
            }];

        VNImageRequestHandler *handler = [[VNImageRequestHandler alloc]
            initWithCIImage:ciImage options:@{}];

        NSError *performError = nil;
        [handler performRequests:@[request] error:&performError];

        if (performError != nil) {
            NSLog(@"[FaceLandmark] Perform error: %@", performError.localizedDescription);
            return NULL;
        }

        if (bestFace == nil || bestFace.landmarks == nil) {
            // 検出なし → JSON で detected=false
            NSString *noFace = @"{\"detected\":false}";
            const char *utf8 = [noFace UTF8String];
            char *result = (char *)malloc(strlen(utf8) + 1);
            strcpy(result, utf8);
            return result;
        }

        // ─── ランドマーク抽出 ───

        CGRect bounds = bestFace.boundingBox; // Vision 正規化座標 (bottom-left原点)
        VNFaceLandmarks2D *lm = bestFace.landmarks;

        // faceBounds → top-left原点に変換
        CGFloat fbX = bounds.origin.x;
        CGFloat fbY = 1.0 - (bounds.origin.y + bounds.size.height); // top-left Y
        CGFloat fbW = bounds.size.width;
        CGFloat fbH = bounds.size.height;

        // 各ランドマークの重心を取得
        CGPoint leftEye = CentroidOfRegion(lm.leftEye, bounds);
        CGPoint rightEye = CentroidOfRegion(lm.rightEye, bounds);
        CGPoint nose = CentroidOfRegion(lm.nose, bounds);
        CGPoint mouth = CentroidOfRegion(lm.outerLips, bounds);

        // 瞳があればそちらを優先
        if (lm.leftPupil != nil && lm.leftPupil.pointCount > 0) {
            leftEye = CentroidOfRegion(lm.leftPupil, bounds);
        }
        if (lm.rightPupil != nil && lm.rightPupil.pointCount > 0) {
            rightEye = CentroidOfRegion(lm.rightPupil, bounds);
        }

        // 頬位置を推定（目と口の中間点）
        CGPoint leftCheek = EstimateCheek(leftEye, mouth, YES);
        CGPoint rightCheek = EstimateCheek(rightEye, mouth, NO);

        // 輪郭（jawline）
        NSMutableString *jawlineStr = [NSMutableString string];
        if (lm.faceContour != nil && lm.faceContour.pointCount > 0) {
            const CGPoint *contourPts = lm.faceContour.normalizedPoints;
            NSUInteger contourCount = lm.faceContour.pointCount;
            [jawlineStr appendString:@"["];
            for (NSUInteger i = 0; i < contourCount; i++) {
                CGPoint pt = LandmarkToNormalized(contourPts[i], bounds);
                if (i > 0) [jawlineStr appendString:@","];
                [jawlineStr appendFormat:@"%.4f,%.4f", pt.x, pt.y];
            }
            [jawlineStr appendString:@"]"];
        } else {
            [jawlineStr appendString:@"[]"];
        }

        // ─── JSON 組み立て ───
        NSString *json = [NSString stringWithFormat:
            @"{"
            "\"detected\":true,"
            "\"faceBoundsX\":%.4f,\"faceBoundsY\":%.4f,"
            "\"faceBoundsW\":%.4f,\"faceBoundsH\":%.4f,"
            "\"leftEyeX\":%.4f,\"leftEyeY\":%.4f,"
            "\"rightEyeX\":%.4f,\"rightEyeY\":%.4f,"
            "\"noseX\":%.4f,\"noseY\":%.4f,"
            "\"mouthX\":%.4f,\"mouthY\":%.4f,"
            "\"leftCheekX\":%.4f,\"leftCheekY\":%.4f,"
            "\"rightCheekX\":%.4f,\"rightCheekY\":%.4f,"
            "\"jawline\":%@"
            "}",
            fbX, fbY, fbW, fbH,
            leftEye.x, leftEye.y,
            rightEye.x, rightEye.y,
            nose.x, nose.y,
            mouth.x, mouth.y,
            leftCheek.x, leftCheek.y,
            rightCheek.x, rightCheek.y,
            jawlineStr
        ];

        NSLog(@"[FaceLandmark] Detection result: %@", json);

        const char *utf8 = [json UTF8String];
        char *result = (char *)malloc(strlen(utf8) + 1);
        strcpy(result, utf8);
        return result;
    }
}

} // extern "C"
