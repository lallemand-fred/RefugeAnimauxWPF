using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class Adoption
    {
        private string ani_identifiant;
        private DateTime date_demande;
        private string statut;
        private int adop_contact;
        // refs typees pour avoir acces aux objets complets
        private Animal animal;
        private Contact contact;


        //constucteur défaut
        public Adoption()
        {
            ani_identifiant = "";
            date_demande = DateTime.Now;
            statut = "";
            adop_contact = 0;
        }
        //consstructeur paramètres (IDs - pour AccesBD)
        public Adoption(string animalId, int contactId, DateTime date)
        {
            this.ani_identifiant = animalId;
            this.adop_contact = contactId;
            this.date_demande = date;
            this.statut = "demande";
        }
        // constructeur types (objets complets - pour les vues)
        public Adoption(Animal animal, Contact contact, DateTime date)
        {
            this.animal = animal;
            this.ani_identifiant = animal.Identifiant;
            this.contact = contact;
            this.adop_contact = contact.Id;
            this.date_demande = date;
            this.statut = "demande";
        }
        //construct copie
        public Adoption(Adoption adoption)
        {
            ani_identifiant = adoption.ani_identifiant;
            date_demande = adoption.date_demande;
            statut = adoption.statut;
            adop_contact = adoption.adop_contact;
            animal = adoption.animal;
            contact = adoption.contact;
        }
        //Propriétés
        
        // AnimalId: clé primaire, ne peut pas être modifié
        public string AnimalId
        {
            get { return this.ani_identifiant; }
            private set { this.ani_identifiant = value; }
        }

        // DateDemande: clé primaire, ne peut pas être modifié
        public DateTime DateDemande
        {
            get { return this.date_demande; }
            private set { this.date_demande = value; }
        }

        // Statut: demande, acceptee, rejet_environnement, rejet_comportement
        public string Statut
        {
            get { return this.statut; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le statut ne peut pas être vide.", nameof(value));

                string v = value.Trim().ToLower();
                if (v != "demande" && v != "acceptee" && v != "rejet_environnement" && v != "rejet_comportement")
                    throw new ArgumentException("Statut invalide. Valeurs: demande, acceptee, rejet_environnement, rejet_comportement", nameof(value));

                this.statut = v;
            }
        }

        public int ContactId
        {
            get { return this.adop_contact; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Le ContactId ne peut pas être vide ou négatif.", nameof(value));
                this.adop_contact = value;
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

        //affiche l adoption
        public override string ToString()
        {
            return ani_identifiant + ", " + date_demande.ToShortDateString() + ", " + statut;
        }
    }

}
