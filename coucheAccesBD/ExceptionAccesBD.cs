using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.coucheAccesBD
{
    public class ExceptionAccesBD : Exception
    {
        private string details;

        //constructeur: ajout détails à l exception standar
        public ExceptionAccesBD(string cause, string details) : base(cause) 
        {
            this.details = details;
        }
        //get récup l erreur
        public string GetDetails() 
        {  
            return details; 
        }
    }
}
