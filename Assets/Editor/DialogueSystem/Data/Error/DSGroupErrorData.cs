using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DS.Data.Error
{
    using Elements;

    public class DSGroupErrorData
    {
        public DSErrorData DSErrorData { get; set; }

        public List<DSGroup> Groups { get; set; } 

        public DSGroupErrorData()
        {
            DSErrorData = new DSErrorData();

            Groups = new List<DSGroup>();
        }

    }

}

