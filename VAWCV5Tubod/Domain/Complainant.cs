using System;
using System.Collections.Generic;
using System.Text;

namespace VAWCV5Tubod.Domain
{
    public class Complainant
    {
        public int CompId { get; set; }
        public byte[]? CompImage { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Sex { get; set; }
        public int Age { get; set; }
        public DateTime BirthDate { get; set; }
        public string Purok { get; set; }
        public string Barangay { get; set; }
        public string Municipal { get; set; }
        public string Province { get; set; }
        public string ContactNo { get; set; }
        public string CivilStatus { get; set; }
        public string EducationalAttainment { get; set; }
        public string Religion { get; set; }
        public string Occupation { get; set; }
        public string Nationality { get; set; }
    }
}
