﻿namespace bakery_web_api.Models.DTO;

public class ChangePasswordRequest
{
    public string UserId { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}