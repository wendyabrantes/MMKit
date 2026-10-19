#import <StoreKit/StoreKit.h>

// Presents Apple's offer-code redemption sheet (iOS 14+). Returns false when unavailable
// so the caller can fall back to the apps.apple.com/redeem link.
extern "C" bool MMKit_PresentCodeRedemptionSheet()
{
    if (@available(iOS 14.0, *))
    {
        dispatch_async(dispatch_get_main_queue(), ^{
            [[SKPaymentQueue defaultQueue] presentCodeRedemptionSheet];
        });
        return true;
    }
    return false;
}
