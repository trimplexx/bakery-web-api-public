﻿namespace bakery_web_api.Models.DTO;

public class EditUserDto
{
    public int UserId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}