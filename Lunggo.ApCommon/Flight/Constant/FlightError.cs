﻿namespace Lunggo.ApCommon.Flight.Constant
{
    public enum FlightError
    {
        TechnicalError = 0,
        FareIdNoLongerValid = 1,
        BookingIdNoLongerValid = 2,
        AlreadyBooked = 3,
        InvalidInputData = 4,
        FailedOnSupplier = 5,
        PartialSuccess = 6,
        NotEligibleToIssue = 7,
        PaymentFailed = 8,
        PriceChanged = 9
    }
}
