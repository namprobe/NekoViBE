namespace NekoViBE.Application.Common.Models;

public class ShippingFeeResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ShippingFeeData? Data { get; set; }
}

public class ShippingFeeData
{
    public int Total { get; set; } // total
    public int ServiceFee { get; set; } // service_fee
    public int InsuranceFee { get; set; } // insurance_fee
    public int PickStationFee { get; set; } // pick_station_fee
    public int CouponValue { get; set; } // coupon_value
    public int R2SFee { get; set; } // r2s_fee
    public int DocumentReturn { get; set; } // document_return
    public int DoubleCheck { get; set; } // double_check
    public int CodFee { get; set; } // cod_fee
    public int PickRemoteAreasFee { get; set; } // pick_remote_areas_fee
    public int DeliverRemoteAreasFee { get; set; } // deliver_remote_areas_fee
    public int CodFailedFee { get; set; } // cod_failed_fee
    public int ReturnAgainFee { get; set; } // return_again_fee (legacy, keep for backward compatibility)
}

