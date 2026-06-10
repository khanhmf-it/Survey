using System;
using System.Collections.Generic;

namespace SURVEY.Model.DTOs;

public partial class authenticationDTO
{
    public int ID { get; set; }

    public string userAdid { get; set; } = null!;
}
