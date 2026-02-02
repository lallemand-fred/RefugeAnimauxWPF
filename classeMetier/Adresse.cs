using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class Adresse
    {
        private string rue;
        private string cp;
        private string localite;

        //constructeur de base
        public Adresse()
        {
            rue = "";
            cp = "";
            localite = "";
        }
        //construc copie de base
        public Adresse(Adresse adresse)
        {
            rue = adresse.rue;
            cp = adresse.cp;
            localite = adresse.localite;
        }
        //constructeur paramètre
        public Adresse(string rue, string cp, string localite)
        {
            this.rue = rue;
            this.cp = cp;
            this.localite = localite;
        }
        // minimum un caractère  
        public string Rue
        {
            get { return this.rue; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length < 1)
                    throw new ArgumentException("La rue doit avoir au moins 1 caractère.", nameof(value));
                this.rue = value;
            }
        }

        // Code postal: 4 caractères
        public string Cp
        {
            get { return this.cp; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length != 4)
                    throw new ArgumentException("Le code postal doit avoir exactement 4 caractères.", nameof(value));
                this.cp = value;
            }
        }
        //minimum 1 caractère
        public string Localite
        {
            get { return this.localite; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && value.Trim().Length < 1)
                    throw new ArgumentException("La localité doit avoir au moins 1 caractère.", nameof(value));
                this.localite = value;
            }
        }

        // ToString pour afficher l adresse
        public override string ToString()
        {
            return rue + ", " + cp + ", " + localite;
        }
    }
}
