namespace RailAppWPF.Classes
{

    // An initial request for Service details

    public class ServiceMetricsRequest
    {

        public string? from_loc { get; set; }
        public string? to_loc { get; set; }
        public string? from_time { get; set; }
        public string? to_time { get; set; }
        public string? from_date { get; set; }
        public string? to_date { get; set; }
        public string? days { get; set; }

    }

    // List of Services

    public class myListOfServices
    {
        public Header header { get; set; }
        public Service[] Services { get; set; }

    }

    public class Header
    {
        public string from_location { get; set; }
        public string to_location { get; set; }
    }

    public class Service
    {
        public Serviceattributesmetrics serviceAttributesMetrics { get; set; }
    }

    public class Serviceattributesmetrics
    {
        public string origin_location { get; set; }
        public string destination_location { get; set; }
        public string gbtt_ptd { get; set; }
        public string gbtt_pta { get; set; }
        public string toc_code { get; set; }
        public string matched_services { get; set; }
        public string[] rids { get; set; }

    }

    // Existing Model

    public class returned_rid
    {

        public string? Retrieved_rid { get; set; }

    }

}

// Request for details of the requested service

public class ServiceDetailsRequest
{
    public string? rid { get; set; }

}

// Single Service Details

public class myService
{
    public Serviceattributesdetails serviceAttributesDetails { get; set; }
}

public class Serviceattributesdetails
{
    public string date_of_service { get; set; }
    public string toc_code { get; set; }
    public string rid { get; set; }
    public Location[] locations { get; set; }
}

public class Location
{
    public string location { get; set; }
    public string gbtt_ptd { get; set; }
    public string gbtt_pta { get; set; }
    public string actual_td { get; set; }
    public string actual_ta { get; set; }
    public string late_canc_reason { get; set; }
}

// Location History

public class location_history
{

    public string Loc { get; set; }
    public string ptd { get; set; }
    public string actual_td { get; set; }
    public string pta { get; set; }
    public string actual_ta { get; set; }

}
