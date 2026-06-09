using System;
using System.Collections.Generic;

namespace SURVEY.Model.Models_SURVEY;

public partial class employee_evaluation
{
    public int ID { get; set; }

    public string? employee_code { get; set; }

    public string? employee_name { get; set; }

    public string? department { get; set; }

    public string? evaluation_period { get; set; }

    public string? g1_good_point { get; set; }

    public int? g1_good_score { get; set; }

    public string? g1_improve_point { get; set; }

    public int? g1_improve_score { get; set; }

    public string? g1_example { get; set; }

    public string? g2_good_point { get; set; }

    public int? g2_good_score { get; set; }

    public string? g2_improve_point { get; set; }

    public int? g2_improve_score { get; set; }

    public string? g2_example { get; set; }

    public string? g3_good_point { get; set; }

    public int? g3_good_score { get; set; }

    public string? g3_improve_point { get; set; }

    public int? g3_improve_score { get; set; }

    public string? g3_example { get; set; }

    public string? g4_good_point { get; set; }

    public int? g4_good_score { get; set; }

    public string? g4_improve_point { get; set; }

    public int? g4_improve_score { get; set; }

    public string? g4_example { get; set; }

    public string? g5_good_point { get; set; }

    public int? g5_good_score { get; set; }

    public string? g5_improve_point { get; set; }

    public int? g5_improve_score { get; set; }

    public string? g5_example { get; set; }

    public string? improvement_proposal { get; set; }

    public double? total_score { get; set; }

    public DateTime? created_at { get; set; }
}
