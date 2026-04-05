using System;
using System.Collections.Generic;
using System.Text;

namespace VAWCV5Tubod.Domain
{
    public class Caselist
    {
        public int CaseId { get; set; }
        public DateTime ComplaintDate { get; set; }
        public int CompId { get; set; }
        public string ComplainantFullname { get; set; }
        public int RespId { get; set; }
        public string RespondentFullname { get; set; }
        public string Violation { get; set; }
        public string SubViolation { get; set; }
        public string SubViolation2 { get; set; }
        public string SubViolation3 { get; set; }
        public string SubViolation4 { get; set; }
        public string CaseStatus { get; set; }
        public string ReferredTo { get; set; }
        public string PlaceOfIncident { get; set; }
        public DateTime IncidentDate { get; set; }
        public string IncidentDescription { get; set; }

    }
}
