using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class FamilleAccueil
    {
        //variables privées
        private string fa_ani_identifiant;
        private DateTime date_debut;
        private DateTime date_fin;
        private int fa_contact;
        // refs typees pour avoir acces aux objets complets
        private Animal animal;
        private Contact contact;

        //constructeur de base
        public FamilleAccueil()
        {
            fa_ani_identifiant = "";
            date_debut = DateTime.Now;
            date_fin = DateTime.MinValue;
            fa_contact = 0;
        }

        //Constructer parametre (IDs - pour AccesBD)
        public FamilleAccueil(string animalId, int contactId, DateTime debut)
        {
            this.fa_ani_identifiant= animalId;
            this.fa_contact = contactId;
            this.date_debut = debut;
            this.date_fin = DateTime.MinValue;
        }

        // constructeur types (objets complets - pour les vues)
        public FamilleAccueil(Animal animal, Contact contact, DateTime debut)
        {
            this.animal = animal;
            this.fa_ani_identifiant = animal.Identifiant;
            this.contact = contact;
            this.fa_contact = contact.Id;
            this.date_debut = debut;
            this.date_fin = DateTime.MinValue;
        }

        // Constructeur copie
        public FamilleAccueil(FamilleAccueil fami)
        {
            fa_ani_identifiant = fami.fa_ani_identifiant;
            date_debut = fami.date_debut;
            date_fin = fami.date_fin;
            fa_contact = fami.fa_contact;
            animal = fami.animal;
            contact = fami.contact;
        }

        //Propriétés 
        // AnimalId: clé primaire, ne peut pas être modifié
        public string AnimalId
        {
            get { return this.fa_ani_identifiant; }
            private set { this.fa_ani_identifiant = value; }
        }

        // ContactId: clé primaire, ne peut pas être modifié
        public int ContactId
        {
            get { return this.fa_contact; }
            private set { this.fa_contact = value; }
        }

        // DateDebut: clé primaire, ne peut pas être modifié
        public DateTime DateDebut
        {
            get { return this.date_debut; }
            private set { this.date_debut = value; }
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

        public DateTime DateFin
        {
            get { return this.date_fin; }
            set
            {
                if (value != DateTime.MinValue && date_debut != DateTime.MinValue && value < date_debut)
                    throw new ArgumentException("La date de fin doit être supérieure ou égale à la date de début.", nameof(value));
                this.date_fin = value;
            }
        }

        // Méthode métier : vérifier si le placement est actif
        public bool EstActif()
        {
            return date_fin == DateTime.MinValue;
        }

        public override string ToString()
        {
            string fin;

            if (date_fin == DateTime.MinValue)
            {
                fin = "En cours";
            }
            else
            {
                fin = date_fin.ToShortDateString();
            }
            return fa_ani_identifiant + ", depuis " + date_debut.ToShortDateString() + ", jusqu'à " + fin;
        }
    }
}
