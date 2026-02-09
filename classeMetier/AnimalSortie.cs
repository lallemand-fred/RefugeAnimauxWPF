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
        // refs typees pour avoir acces aux objets complets
        private Animal animal;
        private Contact contact;

        //constructeur defaut
        public AnimalSortie()
        {
            ani_identifiant = "";
            date_sortie = DateTime.Now;
            raison = "";
            sortie_contact = 0;
        }

        //constructeur paramètres (IDs - pour AccesBD)
        public AnimalSortie(string animalId, DateTime dateSortie, string raison, int contactId)
        {
            this.ani_identifiant= animalId;
            this.date_sortie= dateSortie;
            this.raison= raison;
            this.sortie_contact = contactId;
        }

        // constructeur types (objets complets - pour les vues)
        public AnimalSortie(Animal animal, DateTime dateSortie, string raison, Contact contact)
        {
            this.animal = animal;
            this.ani_identifiant = animal.Identifiant;
            this.date_sortie = dateSortie;
            this.raison = raison;
            this.contact = contact;
            this.sortie_contact = contact.Id;
        }

        //constructeur copie
        public AnimalSortie (AnimalSortie sortie)
        {
            ani_identifiant = sortie.ani_identifiant;
            date_sortie= sortie.date_sortie;
            raison = sortie.raison;
            sortie_contact= sortie.sortie_contact;
            animal = sortie.animal;
            contact = sortie.contact;
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

        // refs typees - donne acces a l'objet complet (peut etre null si cree depuis BD)
        public Animal Animal
        {
            get { return this.animal; }
            set { this.animal = value; }
        }

        public Contact Contact
        {
            get { return this.contact; }
            set { this.contact = value; }
        }

        public override string ToString()
        {
            return ani_identifiant + ", " + date_sortie.ToShortDateString() + ", " + raison;
        }
    }
}
