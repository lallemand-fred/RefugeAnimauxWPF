using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class Vaccination
    {
        private string vac_animal;
        private string vac_vaccin;
        private DateTime vaccination_date;

        //construct de base
        public Vaccination()
        {
            vac_animal = "";
            vac_vaccin = "";
            vaccination_date = DateTime.Now;
        }
        // construct & param
        public Vaccination(string animalId, string vaccin, DateTime date)
        {
            this.vac_animal = animalId;
            this.vac_vaccin = vaccin;
            this.vaccination_date = date;
        }
        //constructeur copie
        public Vaccination(Vaccination vac)
        {
            vac_animal = vac.vac_animal;
            vac_vaccin = vac.vac_vaccin;
            vaccination_date = vac.vaccination_date;
        }
        //Propriétés - Clés primaires
        // AnimalId: clé primaire, ne peut pas être modifié
        public string AnimalId
        {
            get { return this.vac_animal; }
            private set { this.vac_animal = value; }
        }

        // Vaccin: clé primaire, ne peut pas être modifié
        public string Vaccin
        {
            get { return this.vac_vaccin; }
            private set { this.vac_vaccin = value; }
        }

        // Date: clé primaire, ne peut pas être modifié
        public DateTime Date
        {
            get { return this.vaccination_date; }
            private set { this.vaccination_date = value; }
        }
        
        public override string ToString()
        {
            return vac_animal + ", " + vac_vaccin + ", " + vaccination_date.ToShortDateString(); //ToShortDateString = Format court ( jj/mm/aaaa )
        }
    }
}
