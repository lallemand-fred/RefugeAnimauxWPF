using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class AnimalEntree
    {
        private string ani_identifiant;
        private DateTime date_entree;
        private string raison;
        private int entree_contact;
        //Construct base qui initialise
        public AnimalEntree()
        {
            ani_identifiant = "";
            date_entree = DateTime.Now;
            raison = "";
            entree_contact = 0;
        }

        // construct paramètre
        public AnimalEntree(string animalId, DateTime dateEntree, string raison,int contactId) 
        {
            this.ani_identifiant= animalId;
            this.date_entree = dateEntree;
            this.raison = raison;
            this.entree_contact = contactId;
        }
        //constructeur copie
        public AnimalEntree (AnimalEntree entree) 
        { 
            ani_identifiant = entree.ani_identifiant;
            date_entree= entree.date_entree;
            raison = entree.raison;
            entree_contact= entree.entree_contact;
        }
        
        //Propriétés 

        // AnimalId: clé primaire, ne peut pas être modifié
        public string AnimalId
        {
            get { return this.ani_identifiant; }
            private set { this.ani_identifiant = value; }
        }

        // DateEntree: clé primaire, ne peut pas être modifié
        public DateTime DateEntree
        {
            get { return this.date_entree; }
            private set { this.date_entree = value; }
        }

        // Raison: abandon, errant, deces_proprietaire, saisie, retour_adoption, retour_famille_accueil
        public string Raison
        {
            get { return this.raison; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("La raison ne peut pas être vide.", nameof(value));

                string v = value.Trim().ToLower();
                if (v != "abandon" && v != "errant" && v != "deces_proprietaire" &&
                    v != "saisie" && v != "retour_adoption" && v != "retour_famille_accueil")
                    throw new ArgumentException("Raison invalide. Valeurs: abandon, errant, deces_proprietaire, saisie, retour_adoption, retour_famille_accueil", nameof(value));

                this.raison = value.Trim().ToLower();
            }
        }

        public int ContactId
        {
            get { return this.entree_contact; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Le ContactId ne peut pas être vide ou négatif.", nameof(value));
                this.entree_contact = value;
            }
        }

        // Affiche
        public override string ToString()
        {
            return ani_identifiant + ", " + date_entree.ToShortDateString() + ", " + raison;
        }
    }
}
