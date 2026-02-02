using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class AnimalSortie
    {
        private string ani_identifiant;
        private DateTime date_sortie;
        private string raison;
        private int sortie_contact;

        //constructeur defaut
        public AnimalSortie() 
        {
            ani_identifiant = "";
            date_sortie = DateTime.Now;
            raison = "";
            sortie_contact = 0;
        }

        //constructeur paramètres
        public AnimalSortie(string animalId, DateTime dateSortie, string raison, int contactId)
        {
            this.ani_identifiant= animalId;
            this.date_sortie= dateSortie;
            this.raison= raison;
            this.sortie_contact = contactId;
        }

        //constructeur copie
        public AnimalSortie (AnimalSortie sortie)
        {
            ani_identifiant = sortie.ani_identifiant;
            date_sortie= sortie.date_sortie;
            raison = sortie.raison;
            sortie_contact= sortie.sortie_contact;
        }

        //Propriétés - Clés primaires
        // AnimalId: clé primaire, ne peut pas être modifié
        public string AnimalId
        {
            get { return this.ani_identifiant; }
            private set { this.ani_identifiant = value; }
        }

        // DateSortie: clé primaire, ne peut pas être modifié
        public DateTime DateSortie
        {
            get { return this.date_sortie; }
            private set { this.date_sortie = value; }
        }

        // Raison: adoption, retour_proprietaire, deces_animal, famille_accueil
        public string Raison
        {
            get { return this.raison; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La raison ne peut pas être vide.", nameof(value));

                string v = value.Trim().ToLower();
                if (v != "adoption" && v != "retour_proprietaire" && v != "deces_animal" && v != "famille_accueil")
                    throw new ArgumentException("Raison invalide. Valeurs: adoption, retour_proprietaire, deces_animal, famille_accueil", nameof(value));

                this.raison = value.Trim().ToLower();
            }
        }

        public int ContactId
        {
            get { return this.sortie_contact; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Le ContactId ne peut pas être vide ou négatif.", nameof(value));
                this.sortie_contact = value;
            }
        }
        public override string ToString()
        {
            return ani_identifiant + ", " + date_sortie.ToShortDateString() + ", " + raison;
        }
    }
}
