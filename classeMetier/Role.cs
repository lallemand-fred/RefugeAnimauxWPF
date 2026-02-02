using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class Role
    {
        private int rol_identifiant;
        private string rol_nom;
        //construct de base
        public Role() 
        {
            rol_identifiant = 0;
            rol_nom = "";
        }
        //construct param
        public Role(int id, string nom)
        {
            if (id <= 0)
                throw new ArgumentException("L'ID ne peut pas être inférieur ou égal à 0.", nameof(id));
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom du rôle ne peut pas être vide.", nameof(nom));

            string v = nom.Trim().ToLower();
            if (v != "benevole" && v != "adoptant" && v != "particulier" && v != "famille_accueil")
                throw new ArgumentException("Le rôle doit être: benevole, adoptant, particulier ou famille_accueil", nameof(nom));

            this.rol_identifiant = id;
            this.rol_nom = v;
        }
        //construct copie
        public Role (Role role) 
        {
            rol_identifiant = role.rol_identifiant;
            rol_nom = role.rol_nom;
        }

        //Propriétés
        // RolIdentifiant: auto-généré, ne peut pas être modifié
        public int RolIdentifiant
        {
            get { return this.rol_identifiant; }
            private set { this.rol_identifiant = value; }
        }

        // Nom: benevole, adoptant, particulier ou famille_accueil uniquement
        public string RolNom
        {
            get { return this.rol_nom; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le nom du rôle ne peut pas être vide.", nameof(value));

                string v = value.Trim().ToLower();
                if (v != "benevole" && v != "adoptant" && v != "particulier" && v != "famille_accueil")
                    throw new ArgumentException("Le rôle doit être: benevole, adoptant, particulier ou famille_accueil", nameof(value));

                this.rol_nom = v;
            }
        }
        public override string ToString()
        {
            return rol_identifiant + ", " + rol_nom;
        }
    }
}
