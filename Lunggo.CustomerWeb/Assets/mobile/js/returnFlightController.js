﻿app.controller('ReturnFlightController', ['$http', '$scope',  '$rootScope','$interval', '$log', function($http, $scope, $rootScope, $interval, $log) {

    // **********
    // on document ready
    angular.element(document).ready(function () {
        var a = $scope.flightFixRequest();
        $rootScope.getCookies();
        //$scope.FlightFunctions.GetFlight('departure');
        $scope.FlightFunctions.GetFlight('return');
    });

    // **********
    // variables
    $scope.trial = 0;
    $scope.returnUrl = "/";
    $scope.PageConfig = $rootScope.PageConfig;
    $scope.PageConfig.Validated = true;
    $scope.PageConfig.ValidateConfirmation = true;
    $scope.PageConfig.Loaded = true;
    $scope.PageConfig.ActiveSection = 'departure';
    $scope.PageConfig.ActiveOverlay = '';
    $scope.PageConfig.Loading = 0;
    $scope.PageConfig.BodyNoScroll = false;
    $scope.PageConfig.Validating = false;
    $scope.PageConfig.ExpiryDate = {
        Expired: false,
        Time: '',
        Start: function () {
            var expiryTime = new Date($scope.PageConfig.ExpiryDate.Time);
            if ($scope.PageConfig.ExpiryDate.Expired || $scope.PageConfig.ExpiryDate.Starting) return;
            $interval(function () {
                $scope.PageConfig.ExpiryDate.Starting = true;
                var nowTime = new Date();
                if (nowTime > expiryTime) {
                    $scope.PageConfig.ExpiryDate.Expired = true;
                }
            }, 1000);
        },
        Starting: true
    }

    $scope.gtmContentType = gtmContentType;
    $scope.gtmDepartingDepartureDate = gtmDepartingDepartureDate;
    $scope.gtmReturningDepartureDate = gtmReturningDepartureDate;
    $scope.gtmOriginAirport = gtmOriginAirport;
    $scope.gtmDestinationAirport = gtmDestinationAirport;
    $scope.gtmDepartingArrivalDate = gtmDepartingArrivalDate;
    $scope.gtmReturningArrivalDate = gtmReturningArrivalDate;
    $scope.gtmNumAdults = gtmNumAdults;
    $scope.gtmNumChildren = gtmNumChildren;
    $scope.gtmNumInfants = gtmNumInfants;
    $scope.gtmTravelClass = gtmTravelClass;
    $scope.gtmPurchaseCurrency = gtmPurchaseCurrency;
    $scope.FlightConfig = [
        {
            Token: '',
            Name: 'departure',
            FlightList: [],
            ActiveFlight: -1,
            DetailFlight: -1,
            //flightSearchParams: $scope.flightFixRequest(),
            FlightRequest: {
                CabinClass: FlightSearchConfig.flightForm.cabin,
                AdultCount: FlightSearchConfig.flightForm.passenger.adult,
                ChildCount: FlightSearchConfig.flightForm.passenger.child,
                InfantCount: FlightSearchConfig.flightForm.passenger.infant,
                TripType: FlightSearchConfig.flightForm.type,
                trips: FlightSearchConfig.flightForm.trips[0],
                Requests: [],
                Completed: [],
                Securecode: FlightSearchConfig.flightForm.SecureCode,
                Progress: 0,
                FinalProgress: 0,
                Pristine: true
            },
            FlightFilter: {
                Transit: [true, true, true],
                DepartureTime: [true, true, true, true],
                ArrivalTime: [true, true, true, true],
                Airline: [],
                AirlineSelected: [],
                Price: {
                    initial: [-1, -1],
                    current: [0, 1000000000],
                    prices: []
                }
            },
            FlightSort: {
                Label: 'price',
                Value: 'netAdultFare',
                Invert: false,
                Set: function (sortBy, invert) {
                    $scope.FlightConfig[0].FlightSort.Label = sortBy;
                    $scope.FlightConfig[0].FlightSort.Invert = invert;
                    switch (sortBy) {
                        case 'price':
                            $scope.FlightConfig[0].FlightSort.Value = 'netAdultFare';
                            break;
                        case 'duration':
                            $scope.FlightConfig[0].FlightSort.Value = 'trips[0].totalDuration';
                            break;
                        case 'airline':
                            $scope.FlightConfig[0].FlightSort.Value = 'trips[0].airlines[0].name';
                            break;
                        case 'departure':
                            $scope.FlightConfig[0].FlightSort.Value = 'trips[0].segments[0].departureTime';
                            break;
                        case 'arrival':
                            $scope.FlightConfig[0].FlightSort.Value = 'trips[0].segments[(trips[0].segments.length-1)].arrivalTime';
                            break;

                    }
                    $scope.SetOverlay('');
                }
            }
        },
        {
            Name: 'return',
            //flightSearchParams: $scope.flightFixRequest(),
            Token: '',
            FlightList: [],
            ActiveFlight: -1,
            DetailFlight: -1,
            FlightRequest: {
                CabinClass: FlightSearchConfig.flightForm.cabin,
                AdultCount: FlightSearchConfig.flightForm.passenger.adult,
                ChildCount: FlightSearchConfig.flightForm.passenger.child,
                InfantCount: FlightSearchConfig.flightForm.passenger.infant,
                TripType: FlightSearchConfig.flightForm.type,
                trips: FlightSearchConfig.flightForm.trips[1],
                Requests: [],
                Completed: [],
                SecureCode: FlightSearchConfig.flightForm.SecureCode,
                Progress: 0,
                FinalProgress: 0,
                Pristine: true
            },
            FlightFilter: {
                Transit: [true, true, true],
                DepartureTime: [true, true, true, true],
                ArrivalTime: [true, true, true, true],
                Airline: [],
                AirlineSelected: [],
                Price: {
                    initial: [-1, -1],
                    current: [0, 1000000000],
                    prices: []
                }
            },
            FlightSort: {
                Label: 'price',
                Value: 'netAdultFare',
                Invert: false,
                Set: function (sortBy, invert) {
                    $scope.FlightConfig[1].FlightSort.Label = sortBy;
                    $scope.FlightConfig[1].FlightSort.Invert = invert;
                    switch (sortBy) {
                        case 'price':
                            $scope.FlightConfig[1].FlightSort.Value = 'netAdultFare';
                            break;
                        case 'duration':
                            $scope.FlightConfig[1].FlightSort.Value = 'trips[0].totalDuration';
                            break;
                        case 'airline':
                            $scope.FlightConfig[1].FlightSort.Value = 'trips[0].airlines[0].name';
                            break;
                        case 'departure':
                            $scope.FlightConfig[1].FlightSort.Value = 'trips[0].segments[0].departureTime';
                            break;
                        case 'arrival':
                            $scope.FlightConfig[1].FlightSort.Value = 'trips[0].segments[(trips[0].segments.length-1)].arrivalTime';
                            break;
                    }
                    $scope.SetOverlay('');
                }
            }
        }
    ];
    
    // *********************************FUNCTIONS TO SHOW DATA/ICON****************************************
    $scope.checkMeal = function (trip) {
        var available = true;
        for (var x = 0; x < trip.segments.length; x++) {
            if (trip.segments[x].hasMeal) {
                available = true;
            }
        }

        return available;
    }

    $scope.minBaggage = function (trip) {
        var listbaggage = [];
        for (var x = 0; x < trip.segments.length; x++) {
            if (trip.segments[x].baggageCapacity != 0) {
                listbaggage.push(trip.segments[x].baggageCapacity);
            }
        }

        var minvalue = Math.min.apply(Math, listbaggage);
        if (minvalue == 'Infinity') {
            return 0;
        }
        return Math.min.apply(Math, listbaggage);
    }

    $scope.checkBaggageNaN = function (val) {
        return Number.isNaN(val);
    }

    $scope.checkTax = function (trip) {
        var valid = true;
        for (var x = 0; x < trip.segments.length; x++) {
            if (trip.segments[x].includingPsc == true) {
                valid = true;
            }
        }
        return valid;
    }

    // get full date time
    $scope.getFullDate = function (dateTime) {
        if (dateTime) {
            dateTime = parseInt(dateTime.substr(0, 4) + '' + dateTime.substr(5, 2) + '' + dateTime.substr(8, 2));
            return dateTime;
        }
    }

    // get overday date
    $scope.getOverdayDate = function (departureDate, arrivalDate) {
        if (departureDate && arrivalDate) {
            departureDate = new Date(departureDate);
            departureDate = Date.UTC(departureDate.getUTCFullYear(), (departureDate.getUTCMonth()), departureDate.getUTCDate());
            arrivalDate = new Date(arrivalDate);
            arrivalDate = Date.UTC(arrivalDate.getUTCFullYear(), (arrivalDate.getUTCMonth()), arrivalDate.getUTCDate());
            var overday = arrivalDate - departureDate;
            overday = overday / 1000 / 60 / 60 / 24;
            if (overday > 0) {
                overday = '+' + overday;
            }
            return overday;
        }
    }

    // get hour and minute for time filtering		
    $scope.getHour = function (dateTime) {
        dateTime = (dateTime.substr(11, 2)) + (dateTime.substr(14, 2));
        return parseInt(dateTime);
    }

    // ms to time
    $scope.msToTime = function (duration) {
        var milliseconds = parseInt((duration % 1000) / 100),
            seconds = parseInt((duration / 1000) % 60),
            minutes = parseInt((duration / (1000 * 60)) % 60),
            hours = parseInt((duration / (1000 * 60 * 60)));
        // hours = parseInt((duration / (1000 * 60 * 60)) % 24);
        // days = parseInt((duration / (1000 * 60 * 60 * 24)));
        hours = hours;
        minutes = minutes;
        seconds = seconds;
        return hours + "j " + minutes + "m";
    }

    $scope.overlapDate = function (onwardArrival, returnDeparture) {
        if (onwardArrival && returnDeparture) {
            onwardArrival = new Date(onwardArrival);
            returnDeparture = new Date(returnDeparture);
            return (returnDeparture <= onwardArrival);
        }
    }

    // *******************************************************END*******************************************
    
    // *********************************FUNCTIONS FOR PAGE/OVERLAY DISPLAY**********************************

    // set overlay
    $scope.SetOverlay = function (overlay) {
        if (!overlay) {
            $scope.PageConfig.ActiveOverlay = '';
            $scope.PageConfig.BodyNoScroll = false;
        } else {
            $scope.PageConfig.ActiveOverlay = overlay;
            $scope.PageConfig.BodyNoScroll = true;
            
        }
    }

    // set popup
    $scope.SetPopup = function(popup) {
        if (!popup) {
            $scope.PageConfig.ActivePopup = '';
            $scope.PageConfig.BodyNoScroll = true;
        } else {
            $scope.PageConfig.ActivePopup = popup;
            $scope.PageConfig.BodyNoScroll = true;
        }
    }

    // set section
    $scope.SetSection = function(section) {
        if (section) {
            $scope.PageConfig.ActiveSection = section;
        }
        $scope.PageConfig.BodyNoScroll = false;
        //$log.debug('Changing section to : '+section);
    }

    // Flip airports in change flight
    $rootScope.flip = function () {
        var temp = $rootScope.FlightSearchForm.AirportOrigin.Code;
        $rootScope.FlightSearchForm.AirportOrigin.Code = $rootScope.FlightSearchForm.AirportDestination.Code;
        $rootScope.FlightSearchForm.AirportDestination.Code = temp;
        temp = $rootScope.FlightSearchForm.AirportOrigin.City;
        $rootScope.FlightSearchForm.AirportOrigin.City = $rootScope.FlightSearchForm.AirportDestination.City;
        $rootScope.FlightSearchForm.AirportDestination.City = temp;
    }

    // *******************************************************END*******************************************

    // ********* FUNCTIONS FOR GETTING FLIGHT, DISPLAYING FLIGHT SEARCH RESULT AND SELECTING FLIGHT ********
    $scope.FlightFunctions = {};

    // Getting search param for API
    $scope.flightFixRequest = function () {
        var cabin = FlightSearchConfig.flightForm.cabin;
        if (cabin != 'y' || cabin != 'c' || cabin != 'f') {
            switch (cabin) {
                case 'Economy':
                    cabin = 'y';
                    break;
                case 'Business':
                    cabin = 'c';
                    break;
                case 'First':
                    cabin = 'f';
                    break;
            }
        }
        var departureTemp = FlightSearchConfig.flightForm;
        var depDate = new Date(departureTemp.trips[0][0].DepartureDate) || '';
        var departureDate = (('0' + depDate.getDate()).slice(-2) + ('0' + (depDate.getMonth() + 1)).slice(-2) + depDate.getFullYear().toString().substr(2, 2));
        var departureRequest = departureTemp.trips[0][0].OriginAirport + departureTemp.trips[0][0].DestinationAirport + departureDate;
        var returnTemp = departureTemp ;
        var retDate = new Date(returnTemp.trips[1][0].DepartureDate) || '';
        var returnDate = (('0' + retDate.getDate()).slice(-2) + ('0' + (retDate.getMonth() + 1)).slice(-2) + retDate.getFullYear().toString().substr(2, 2));
        var returnRequest = returnTemp.trips[1][0].OriginAirport + returnTemp.trips[1][0].DestinationAirport + returnDate;
        
        var passenger = FlightSearchConfig.flightForm.passenger.adult + '' + FlightSearchConfig.flightForm.passenger.child + '' + FlightSearchConfig.flightForm.passenger.infant + cabin;
        return departureRequest + '~' + returnRequest + '-' + passenger;
    }
    
    //Arrange flight results
    $scope.arrangeFlightData = function (targetScope, data) {
        if (targetScope == "departure" || targetScope == "Departure") {
            targetScope = $scope.FlightConfig[0];
        } else {
            targetScope = $scope.FlightConfig[1];
        }

        var startNumber = targetScope.FlightList.length;

        for (var i = 0; i < data.length; i++) {
            data[i].Available = true;
            data[i].IndexNo = (startNumber + i);
            targetScope.FlightList.push(data[i]);
            for (var x = 0; x < data[i].trips[0].airlines.length; x++) {
                data[i].trips[0].airlines[x].Checked = true;
            }
        }

        //if (targetScope.FlightRequest.Progress == 100) {

            // start expiry date
            $scope.PageConfig.ExpiryDate.Start();

            // loop the result
            for (var i = 0; i < targetScope.FlightList.length; i++) {
                // populate prices
                targetScope.FlightFilter.Price.prices.push(targetScope.FlightList[i].netAdultFare);

                // populate airline code
                targetScope.FlightList[i].AirlinesTag = [];
                for (var x = 0; x < targetScope.FlightList[i].trips[0].airlines.length; x++) {
                    targetScope.FlightFilter.Airline.push(targetScope.FlightList[i].trips[0].airlines[x]);
                    targetScope.FlightList[i].AirlinesTag.push(targetScope.FlightList[i].trips[0].airlines[x].name);
                }
            }

            function sortNumber(a, b) {
                return a - b;
            }

            targetScope.FlightFilter.Price.prices.sort(sortNumber);
            targetScope.FlightFilter.Price.initial[0] = Math.floor(targetScope.FlightFilter.Price.prices[0]);
            targetScope.FlightFilter.Price.initial[1] = Math.round(targetScope.FlightFilter.Price.prices[targetScope.FlightFilter.Price.prices.length - 1]);

            targetScope.FlightFilter.Price.current[0] = Math.floor(targetScope.FlightFilter.Price.prices[0]);
            targetScope.FlightFilter.Price.current[1] = Math.round(targetScope.FlightFilter.Price.prices[targetScope.FlightFilter.Price.prices.length - 1]);
            
            $('.departure-price-slider').slider({
                range: true,
                min: $scope.FlightConfig[0].FlightFilter.Price.initial[0],
                max: $scope.FlightConfig[0].FlightFilter.Price.initial[1],
                step: 100,
                values: [$scope.FlightConfig[0].FlightFilter.Price.initial[0], $scope.FlightConfig[0].FlightFilter.Price.initial[1]],
                create: function (event, ui) {
                    $('.departure-price-slider-min').val($scope.FlightConfig[0].FlightFilter.Price.initial[0]);
                    $('.departure-price-slider-min').trigger('input');
                    $('.departure-price-slider-max').val($scope.FlightConfig[0].FlightFilter.Price.initial[1]);
                    $('.departure-price-slider-max').trigger('input');
                },
                slide: function (event, ui) {
                    $('.departure-price-slider-min').val(ui.values[0]);
                    $('.departure-price-slider-min').trigger('input');
                    $('.departure-price-slider-max').val(ui.values[1]);
                    $('.departure-price-slider-max').trigger('input');
                }
            });

            $('.return-price-slider').slider({
                range: true,
                min: $scope.FlightConfig[1].FlightFilter.Price.initial[0],
                max: $scope.FlightConfig[1].FlightFilter.Price.initial[1],
                step: 100,
                values: [$scope.FlightConfig[1].FlightFilter.Price.initial[0], $scope.FlightConfig[1].FlightFilter.Price.initial[1]],
                create: function (event, ui) {
                    $('.return-price-slider-min').val($scope.FlightConfig[1].FlightFilter.Price.initial[0]);
                    $('.return-price-slider-min').trigger('input');
                    $('.return-price-slider-max').val($scope.FlightConfig[1].FlightFilter.Price.initial[1]);
                    $('.return-price-slider-max').trigger('input');
                },
                slide: function (event, ui) {
                    $('.return-price-slider-min').val(ui.values[0]);
                    $('.return-price-slider-min').trigger('input');
                    $('.return-price-slider-max').val(ui.values[1]);
                    $('.return-price-slider-max').trigger('input');
                }
            });

            var dupes = {};
            var Airlines = [];
            $.each(targetScope.FlightFilter.Airline, function (i, el) {
                if (!dupes[el.name]) {
                    dupes[el.name] = true;
                    Airlines.push(el);
                }
            });
            targetScope.FlightFilter.Airline = Airlines;
            Airlines = [];
            //targetScope.FlightFiltering.AirlineCheck('departure');
        }

    // get  flight
    $scope.listPricesDeparting = [];
    $scope.listPricesReturning = [];
    $scope.FlightFunctions.GetFlight = function (targetScope) {
        if ($scope.trial > 3) {
            $scope.trial = 0;
        }
        $scope.PageConfig.Busy = true;
        var requestReturnParams = $scope.flightFixRequest();
        targetScope = $scope.FlightConfig[1];
        //$log.debug('Getting flight for : ' + targetScope.Name + ' . Request : '+targetScope.FlightRequest.Requests);
        if (targetScope.FlightRequest.Progress < 100) {

            // **********
            // ajax
                var authAccess = getAuthAccess();
                if (authAccess == 1 || authAccess == 2)
                {
                    $http({
                        method: 'GET',
                        url: FlightSearchConfig.Url + requestReturnParams + '/' + targetScope.FlightRequest.Progress,
                        headers: { 'Authorization': 'Bearer ' + getCookie('accesstoken') }

                    }).then(function (returnData) {

                        // set searchID
                        RevalidateConfig.SearchId = $scope.flightFixRequest();
                        if (!$scope.PageConfig.ExpiryDate) {
                            $scope.PageConfig.ExpiryDate.Time = returnData.data.expTime;
                            $scope.FlightConfig[0].FlightRequest.FinalProgress = targetScope.progress;
                        }

                        if (targetScope.FlightRequest.Progress < 100) {
                            targetScope.FlightRequest.FinalProgress = targetScope.FlightRequest.Progress; // change this
                            $scope.FlightConfig[0].FlightRequest.FinalProgress = targetScope.FlightRequest.Progress;
                        }

                        targetScope.FlightRequest.Progress = returnData.data.progress;
                        $scope.FlightConfig[0].FlightRequest.Progress = returnData.data.progress;

                        // if granted request is not null
                        if (returnData.data.flights) {
                            // update total progress
                            targetScope.FlightRequest.Progress = returnData.data.progress;
                            $log.debug('Progress : ' + targetScope.FlightRequest.Progress + ' %');
                            //$log.debug(returnData);

                            if (returnData.data.flights.length != 0) {
                                if (returnData.data.flights[0].options.length) {
                                    $scope.arrangeFlightData('departure', returnData.data.flights[0].options); // For Departure Flight
                                    $scope.FlightFiltering.AirlineCheck('departure');
                                    for (var x = 0; x < returnData.data.flights[0].options.length; x++) {
                                        $scope.listPricesDeparting.push(parseInt(returnData.data.flights[0].options[x].netTotalFare));
                                    }
                                }
                                if (returnData.data.flights[1].options.length) {
                                    $scope.arrangeFlightData('return', returnData.data.flights[1].options); // For Return Flight
                                    $scope.FlightFiltering.AirlineCheck('return');
                                    for (var x = 0; x < returnData.data.flights[1].options.length; x++) {
                                        $scope.listPricesReturning.push(parseInt(returnData.data.flights[1].options[x].netTotalFare));
                                    }
                                }
                            }
                        }
                        // loop the function
                        setTimeout(function () {
                            $scope.FlightFunctions.GetFlight(targetScope.Name);
                        }, 1000);
                    }).catch(function () {
                        $scope.trial++;
                        if (refreshAuthAccess() && $scope.trial < 4) //refresh cookie
                        {
                            $scope.FlightFunctions.GetFlight(targetScope.Name);
                        }
                        else {
                            $log.debug('Failed to get flight list');
                            for (var i = 0; i < targetScope.FlightRequest.Requests.length; i++) {
                                // add to completed
                                if (targetScope.FlightRequest.Completed.indexOf(targetScope.FlightRequest.Requests[i] < 0)) {
                                    targetScope.FlightRequest.Completed.push(targetScope.FlightRequest.Requests[i]);
                                }// check current request. Remove if completed
                                if (targetScope.FlightRequest.Requests.indexOf(targetScope.FlightRequest.Requests[i] < 0)) {
                                    targetScope.FlightRequest.Requests.splice(targetScope.FlightRequest.Requests.indexOf(targetScope.FlightRequest.Requests[i]), 1);
                                }
                            }
                            targetScope.FlightRequest.Progress = 100;
                            targetScope.FlightRequest.FinalProgress = 100;
                        }
                    });
                }
                else
                {
                    targetScope.FlightRequest.Progress = 100;
                    targetScope.FlightRequest.FinalProgress = 100;
                    $log.debug('Not Authorized');
                }
        } else {
            $log.debug('Finished getting flight list !');
            $scope.PageConfig.Busy = true;

            !function (f, b, e, v, n, t, s) {
                if (f.fbq) return; n = f.fbq = function () {
                    n.callMethod ?
                    n.callMethod.apply(n, arguments) : n.queue.push(arguments)
                }; if (!f._fbq) f._fbq = n;
                n.push = n; n.loaded = !0; n.version = '2.0'; n.queue = []; t = b.createElement(e); t.async = !0;
                t.src = v; s = b.getElementsByTagName(e)[0]; s.parentNode.insertBefore(t, s)
            }(window, document, 'script', '//connect.facebook.net/en_US/fbevents.js');

            //fbq('init', '<FB_PIXEL_ID>');
            var lowestPriceDep, lowestPriceRet, lowestPrice;

            if ($scope.listPricesDeparting.length > 0) {
                lowestPriceDep = Math.min.apply(Math, $scope.listPricesDeparting);
            } else {
                lowestPriceDep = 0;
            }

            if ($scope.listPricesReturning.length > 0) {
                lowestPriceRet = Math.min.apply(Math, $scope.listPricesReturning);
            } else {
                lowestPriceRet = 0;
            }

            if (lowestPriceDep != 0 && lowestPriceRet != 0) {
                lowestPrice = lowestPriceRet + lowestPriceDep;
            } else {
                lowestPrice = 0;
            }

            fbq('track', 'Search', {
                content_type: $scope.gtmContentType,
                departing_departure_date: $scope.gtmDepartingDepartureDate,
                returning_departure_date: $scope.gtmReturningDepartureDate,
                departing_arrival_date: $scope.gtmDepartingArrivalDate,
                returning_arrival_date: $scope.gtmReturningArrivalDate,
                origin_airport: $scope.gtmOriginAirport,
                destination_airport: $scope.gtmDestinationAirport,
                num_adults: $scope.gtmNumAdults,
                num_children: $scope.gtmNumChildren,
                num_infants: $scope.gtmNumInfants,
                travel_class: $scope.gtmTravelClass,
                purchase_value: lowestPrice,
                purchase_currency: $scope.gtmPurchaseCurrency,
            });
        }
    }

    // arrange flight TO BE DELETED
    //$scope.FlightFunctions.GenerateFlightList = function(targetScope, data) {
    //    targetScope = targetScope == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1] ;
    //    var startNo = targetScope.FlightList.length;
    //    for (var i = 0; i < data.length; i++) {
    //        data[i].Available = true;
    //        data[i].IndexNo = (startNo + i);
    //        // init airlines
    //        for (var x = 0; x < data[i].trips[0].airlines.length; x++) {
    //            data[i].trips[0].airlines[x].Checked = true;
    //        }
    //        targetScope.FlightList.push(data[i]);
    //    }
    //}

    //// run if departure and return flight search has completed TO BE DELETED
    //$scope.FlightFunctions.CompleteGetFlight = function (targetScope) {
    //    $log.debug('/--------------------------/');
    //    $log.debug('Post get flight for : '+targetScope);
    //    targetScope = targetScope == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1];
    //    // generate airline filter
    //    // generate airline for flight filtering		
    //    for (var i = 0; i < targetScope.FlightList.length; i++) {
    //        targetScope.FlightList[i].AirlinesTag = [];
    //        for (var x = 0; x < targetScope.FlightList[i].trips[0].airlines.length; x++) {
    //            targetScope.FlightList[i].AirlinesTag.push(targetScope.FlightList[i].trips[0].airlines[x].name);
    //            targetScope.FlightFilter.Airline.push(targetScope.FlightList[i].trips[0].airlines[x]);
    //        }
    //    }
    //    // remove duplicate from airline filter		
    //    var dupes = {};
    //    var Airlines = [];
    //    $.each(targetScope.FlightFilter.Airline, function (i, el) {
    //        if (!dupes[el.name]) {
    //            dupes[el.name] = true;
    //            Airlines.push(el);
    //        }
    //    });
    //    targetScope.FlightFilter.Airline = Airlines;
    //    Airlines = []; // empty the variable
    //    //$log.debug(targetScope);
    //}

    // set active flight
    $scope.FlightFunctions.SetActiveFlight = function (targetScope, flightNumber) {
        if (targetScope) {
            targetScope = targetScope == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1];
            if (flightNumber >= 0) {
                targetScope.ActiveFlight = flightNumber;

                if ($scope.FlightConfig[0].ActiveFlight != -1 && $scope.FlightConfig[1].ActiveFlight != -1) {
                    $scope.SetOverlay('summary'); 
                    //$scope.PageConfig.Validating = true;
                    //$scope.FlightFunctions.Revalidate($scope.FlightConfig[0].ActiveFlight, $scope.FlightConfig[1].ActiveFlight);
                } else if ($scope.FlightConfig[0].ActiveFlight >= 0 && $scope.FlightConfig[1].ActiveFlight < 0) {
                    $scope.SetPopup('roundtrip-return');
                } else if ($scope.FlightConfig[0].ActiveFlight < 0 && $scope.FlightConfig[1].ActiveFlight >= 0) {
                    $scope.SetPopup('roundtrip-departure');
                }
            } else {
                targetScope.ActiveFlight = -1;
            }
        }       
    }

    // swap flight DO NOT DELETE
    $scope.FlightFunctions.SwapFlight = function() {
        if ($scope.PageConfig.ActiveSection == 'departure') {
            $scope.SetPopup('roundtrip-return');
        } else {
            $scope.SetPopup('roundtrip-departure');
        }
        $scope.SetOverlay('changeflight');
    }

    // show flight detail
    $scope.FlightFunctions.ShowDetail = function (targetScope, flightNumber) {      
        targetScope = targetScope == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1];
        // set detail flight
        targetScope.DetailFlight = flightNumber;
        $scope.SetOverlay('flight-detail');
    }

    $scope.selectError = false;
    // revalidate flight
    $scope.FlightFunctions.Revalidate = function(departureIndexNo, returnIndexNo) {

        $scope.PageConfig.Validating = true;
        $log.debug('Validating flight no : ' + departureIndexNo + ' & ' + returnIndexNo);

        // **********
        // after departure flight and return flight validated
        var afterValidate = function () {
            $log.debug('Flights validated');
            $scope.PageConfig.Validated = true;

            // if both flight available
            if ($scope.FlightConfig[0].FlightAvailable && $scope.FlightConfig[1].FlightAvailable) {
                $log.debug('Flights available. Will be redirected shortly');
                var fareToken = $scope.FlightConfig[0].Token;
                $('.pushToken .fareToken').val(fareToken);
                $('.pushToken').submit();
            } else {
                if ((!$scope.FlightConfig[0].FlightAvailable && !$scope.FlightConfig[0].FlightNewPrice) || (!$scope.FlightConfig[1].FlightAvailable && !$scope.FlightConfig[1].FlightNewPrice)) {
                    $scope.PageConfig.ValidateConfirmation = true;
                } else {
                    $scope.PageConfig.ValidateConfirmation = true;
                }
                $scope.PageConfig.Validating = true;
            }
        }
        // **********
        // validate flight function
        
        if ($scope.trial > 3) {
            $scope.trial = 0;
        }
        
        var authAccess = getAuthAccess();
        if (authAccess == 1 || authAccess == 2)
        {
            $http({
                method: 'POST',
                url: SelectConfig.Url,
                data: {
                    searchId: $scope.flightFixRequest(),
                    regs: [$scope.FlightConfig[0].FlightList[departureIndexNo].reg, $scope.FlightConfig[1].FlightList[returnIndexNo].reg],
                },
                headers: { 'Authorization': 'Bearer ' + getCookie('accesstoken') }
            }).then(function (returnData) {

                $scope.FlightConfig[0].FlightValidating = true;
                $scope.FlightConfig[1].FlightValidating = true;
                $scope.FlightConfig[0].FlightValidated = true;
                $scope.FlightConfig[1].FlightValidated = true;

                if ((returnData.data.token != "" || returnData.data.token != null) && returnData.data.status == "200") {
                    $log.debug('flight available');
                    $scope.FlightConfig[0].FlightAvailable = true;
                    $scope.FlightConfig[0].Token = returnData.data.token;
                    $scope.FlightConfig[1].FlightAvailable = true;
                    $scope.FlightConfig[1].Token = returnData.data.token;
                }
                else {
                    $log.debug('flight unavailable');
                    $scope.FlightConfig[0].validateAvailable = true;
                    $scope.FlightConfig[1].validateAvailable = true;
                    $scope.selectError = true;
                }

                afterValidate();
                
            }).catch(function (returnData) {
                $scope.trial++;
                if (refreshAuthAccess() && $scope.trial < 4) //refresh cookie
                {
                    $scope.FlightFunctions.Revalidate(departureIndexNo, returnIndexNo);
                }
                else {
                    $log.debug('ERROR Validating Flight');
                    $log.debug('--------------------');
                    $scope.selectError = true;
                }
                    
            });
        }
        else
        {
            $scope.selectError = true;
            $scope.FlightConfig[0].validateAvailable = true;
            $scope.FlightConfig[1].validateAvailable = true;
            $log.debug('Not Authorized');
        }
    }

    $scope.FlightFunctions.ResetValidation = function () {
        $scope.PageConfig.ValidateConfirmation = true;
        $scope.PageConfig.Validating = true;
        $scope.PageConfig.Validated = true;
        $scope.FlightConfig[0].FlightValidated = true;
        $scope.FlightConfig[1].FlightValidated = true;
    }

    // *******************************************************END*******************************************

    // ********************************************FLIGHT FILTER********************************************
    // flight filtering functions
    $scope.FlightFiltering = {};
    $scope.FlightFiltering.Touched = [false, false];
    // available filter		
    $scope.FlightFiltering.AvailableFilter = function () {
        return function (flight) {
            if (flight.Available) {
                return flight;
            }
        }
    }

    // price filter
    $scope.priceFilter = function(targetFlight) {
        return function (flight) {
            if (targetFlight == 'departure') {
                if (flight.netAdultFare >= $scope.FlightConfig[0].FlightFilter.Price.current[0] && flight.netAdultFare <= $scope.FlightConfig[0].FlightFilter.Price.current[1]) {
                    return flight;
                }
            } else {
                if (flight.netAdultFare >= $scope.FlightConfig[1].FlightFilter.Price.current[0] && flight.netAdultFare <= $scope.FlightConfig[1].FlightFilter.Price.current[1]) {
                    return flight;
                }
            }        
        }
    }

    // transit filter		
    $scope.FlightFiltering.TransitFilter = function (targetFlight) {
        var targetScope = (targetFlight == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1]);
        return function (flight) {
            if (targetScope.FlightFilter.Transit[0]) {
                if (flight.trips[0].transitCount == 0) {
                    return flight;
                }
            }

            if (targetScope.FlightFilter.Transit[1]) {
                if (flight.trips[0].transitCount == 1) {
                    return flight;
                }
            }
            if (targetScope.FlightFilter.Transit[2]) {
                if (flight.trips[0].transitCount > 1) {
                    return flight;
                }
            }
        }
    }

    // departure time filter		
    $scope.FlightFiltering.DepartureTimeFilter = function (targetFlight) {
        var targetScope = (targetFlight == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1]);
        return function (flight) {
            if (targetScope.FlightFilter.DepartureTime[0]) {
                if ($scope.getHour(flight.trips[0].segments[0].departureTime) >= 0400 && $scope.getHour(flight.trips[0].segments[0].departureTime) <= 1100) {
                    return flight;
                }
            }

            if (targetScope.FlightFilter.DepartureTime[1]) {
                if ($scope.getHour(flight.trips[0].segments[0].departureTime) >= 1100 && $scope.getHour(flight.trips[0].segments[0].departureTime) <= 1500) {
                    return flight;
                }
            }

            if (targetScope.FlightFilter.DepartureTime[2]) {
                if ($scope.getHour(flight.trips[0].segments[0].departureTime) >= 1500 && $scope.getHour(flight.trips[0].segments[0].departureTime) <= 1900) {
                    return flight;
                }
            }

            if (targetScope.FlightFilter.DepartureTime[3]) {
                if ($scope.getHour(flight.trips[0].segments[0].departureTime) >= 1900 || $scope.getHour(flight.trips[0].segments[0].departureTime) <= 0400) {
                    return flight;
                }
            }           
        }
    }

    // arrival time filter		
    $scope.FlightFiltering.ArrivalTimeFilter = function (targetFlight) {
        var targetScope = (targetFlight == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1]);
        return function (flight) {
            if (targetScope.FlightFilter.ArrivalTime[0]) {
                if ($scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) >= 0400 && $scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) <= 1100) {
                    return flight;
                }
            }
            if (targetScope.FlightFilter.ArrivalTime[1]) {
                if ($scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) >= 1100 && $scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) <= 1500) {
                    return flight;
                }
            }
            if (targetScope.FlightFilter.ArrivalTime[2]) {
                if ($scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) >= 1500 && $scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) <= 1900) {
                    return flight;
                }
            }
            if (targetScope.FlightFilter.ArrivalTime[3]) {
                if ($scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) >= 1900 || $scope.getHour(flight.trips[0].segments[flight.trips[0].segments.length - 1].arrivalTime) <= 0400) {
                    return flight;
                }
            }
        }
    }

    // airline filter		
    $scope.FlightFiltering.AirlineCheck = function (targetFlight) {
        var targetScope = (targetFlight == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1]);
        targetScope.FlightFilter.AirlineSelected = [];
        for (var i = 0; i < targetScope.FlightFilter.Airline.length; i++) {
            if (targetScope.FlightFilter.Airline[i].Checked) {
                targetScope.FlightFilter.AirlineSelected.push(targetScope.FlightFilter.Airline[i].name);
            }
        }
    }

    $scope.FlightFiltering.AirlineFilter = function (targetFlight) {
        var targetScope = (targetFlight == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1]);
        var touched;
        return function (flight) {
            if (targetScope.Name == 'departure') {
                touched = $scope.FlightFiltering.Touched[0];
            } else {
                touched = $scope.FlightFiltering.Touched[1];
            }
            if (touched == true) {
                return flight;
            } else {
                for (var i = 0; i < flight.AirlinesTag.length; i++) {
                    if (targetScope.FlightFilter.AirlineSelected.indexOf(flight.AirlinesTag[i]) != -1) {
                        return flight;
                    }
                }
            }
        }
    }

    $scope.FlightFiltering.ResetFilter = function(targetFlight) {
        var targetScope = (targetFlight == 'departure' ? $scope.FlightConfig[0] : $scope.FlightConfig[1]);
        targetScope.FlightFilter.TransitFilter = [true, true, true];
        targetScope.FlightFilter.DepartureTime = [true, true, true, true];
        targetScope.FlightFilter.ArrivalTime = [true, true, true, true];
        targetScope.FlightFilter.Price.current[0] = targetScope.FlightFilter.Price.initial[0];
        targetScope.FlightFilter.Price.current[1] = targetScope.FlightFilter.Price.initial[1];
        for (var i = 0; i < targetScope.FlightFilter.Airline.length; i++) {
            targetScope.FlightFilter.Airline[i].Checked = true;
            targetScope.FlightFilter.AirlineSelected.push(targetScope.FlightFilter.Airline[i].name);
        }
        if (targetFlight == 'departure') {
            $('.departure-price-slider').slider("values", 0, targetScope.FlightFilter.Price.initial[0]);
            $('.departure-price-slider').slider("values", 1, targetScope.FlightFilter.Price.initial[1]);
        } else {
            $('.return-price-slider').slider("values", 0, targetScope.FlightFilter.Price.initial[0]);
            $('.return-price-slider').slider("values", 1, targetScope.FlightFilter.Price.initial[1]);
        }
    }

    // *****************************************************END*******************************************
}]);