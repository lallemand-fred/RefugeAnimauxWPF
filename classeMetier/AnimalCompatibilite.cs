using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class AnimalCompatibilite
    {
        //propriétés
        private string ani_identifiant;
        private int comp_type; // FK vers COMPATIBILITE.comp_identifiant
        private string type_nom; // nom du type (chat, chien, poney, etc.) récupéré depuis COMPATIBILITE
        private bool valeur; // true=compatible, false=pas compatible
        private string description; // optionnel
        // ref typee pour avoir acces a l'objet complet
        private Animal animal;

        //constructeur de base
        public AnimalCompatibilite()
        {
            ani_identifiant = "";
            comp_type = 0;
            type_nom = "";
            valeur = false;
            description = "";
        }

        //constructeur paramétré (3 paramètres)
        public AnimalCompatibilite(string animalId, int compType, bool valeur)
        {
            this.ani_identifiant = animalId;
            this.comp_type = compType;
            this.type_nom = "";
            this.valeur = valeur;
            this.description = "";
        }

        //constructeur paramétré (4 paramètres avec description)
        public AnimalCompatibilite(string animalId, int compType, bool valeur, string description)
        {
            this.ani_identifiant = animalId;
            this.comp_type = compType;
            this.type_nom = "";
            this.valeur = valeur;
            this.description = description;
        }

        //constructeur paramétré (5 paramètres avec description + typeNom depuis BD)
        public AnimalCompatibilite(string animalId, int compType, string typeNom, bool valeur, string description)
        {
            this.ani_identifiant = animalId;
            this.comp_type = compType;
            this.type_nom = typeNom;
            this.valeur = valeur;
            this.description = description;
        }

        // constructeur type (objet complet - pour les vues)
        public AnimalCompatibilite(Animal animal, int compType, string typeNom, bool valeur, string description)
        {
            this.animal = animal;
            this.ani_identifiant = animal.Identifiant;
            this.comp_type = compType;
            this.type_nom = typeNom;
            this.valeur = valeur;
            this.description = description;
        }

        //constructeur copie
        public AnimalCompatibilite(AnimalCompatibilite comp)
        {
            this.ani_identifiant = comp.ani_identifiant;
            this.comp_type = comp.comp_type;
            this.type_nom = comp.type_nom;
            this.valeur = comp.valeur;
            this.description = comp.description;
            this.animal = comp.animal;
        }

        //Propriétés - Clés primaires
        // AnimalId: clé primaire, ne peut pas être modifié
        public string AnimalId
        {
            get { return this.ani_identifiant; }
            private set { this.ani_identifiant = value; }
        }

        // CompType: clé primaire, ne peut pas être modifié
        public int CompType
        {
            get { return this.comp_type; }
            private set { this.comp_type = value; }
        }

        // TypeNom: nom du type de compatibilité (récupéré depuis BD)
        public string TypeNom
        {
            get { return this.type_nom; }
            set { this.type_nom = value; }
        }

        public bool Valeur
        {
            get { return this.valeur; }
            set { this.valeur = value; }
        }

        public string Description
        {
            get { return this.description; }
            set { this.description = value; }
        }

        // ref typee - donne acces a l'objet complet (peut etre null si cree depuis BD)
        public Animal Animal
        {
            get { return this.animal; }
            set { this.animal = value; }
        }

        //Méthode métier
        public string AffichageCompatibilite()
        {
            if (valeur)
                return "Compatible";
            else
                return "Pas compatible";
        }

        //ToString
        public override string ToString()
        {
            // affiche le nom du type si disponible, sinon le numéro
            string typeDisplay = !string.IsNullOrWhiteSpace(type_nom)
                ? type_nom
                : "N°" + comp_type;

            return ani_identifiant + ", Compatibilité avec " + typeDisplay + ": " + AffichageCompatibilite();
        }
    }
}
