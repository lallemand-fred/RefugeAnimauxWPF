using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class Compatibilite
    {
        private int comp_identifiant;
        private string type;

        //constructeru de base
        public Compatibilite()
        {
            this.comp_identifiant = 0;
            this.type = "";
        }
        //constructeur paramètres
        public Compatibilite(int id, string type)
        {
            if (id <= 0)
                throw new ArgumentException("L'ID ne peut pas être inférieur ou égal à 0.", nameof(id));
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("Le type ne peut pas être vide.", nameof(type));

            string v = type.Trim().ToLower();
            if (v != "chat" && v != "chien" && v != "jeune enfant" && v != "enfant" && v != "jardin" && v != "poney")
                throw new ArgumentException("Type invalide.", nameof(type));

            this.comp_identifiant = id;
            this.type = v;
        }
        //construc copie
        public Compatibilite(Compatibilite comp)
        {
            comp_identifiant = comp.comp_identifiant;
            type = comp.type;
        }
        //Propriétés
        // Id: auto-généré, ne peut pas être modifié
        public int Id
        {
            get { return this.comp_identifiant; }
            private set { this.comp_identifiant = value; }
        }

        // Type: chat, chien, jeune enfant, enfant, jardin, poney
        public string TypeCompatibilite
        {
            get { return this.type; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le type de compatibilité ne peut pas être vide.", nameof(value));

                string v = value.Trim().ToLower();
                if (v != "chat" && v != "chien" && v != "jeune enfant" && v != "enfant" && v != "jardin" && v != "poney")
                    throw new ArgumentException("Type invalide. Valeurs: chat, chien, jeune enfant, enfant, jardin, poney", nameof(value));

                this.type = value.Trim().ToLower();
            }
        }

        public override string ToString() 
        {
            return comp_identifiant + ", " + type;
        }
    }
}
