using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class Animal
    {
        //Séquence automatique pour générer les identifiants
        private static int prochainNumeroSequence = 1;
        private static string derniereDate = ""; // mémorise la date du dernier ID généré (format yyMMdd)

        //propriétés
        private string identifiant;//11 chiffres, format yymmdd99999
        private string nom; //minimum 2 caractères
        private string type; //chat ou chien
        private char sexe; // M ou F
        private bool sterilise;
        private DateTime date_sterilisation;//supérieur ou égal à la date de naissance
        private DateTime date_naissance;    //plus petit ou égal à aujourd'hui
        private DateTime date_deces;        //supérieur ou égal à la date de naissance.
        private string race;
        private string particularites;
        private List<string> couleurs; 
        private List<AnimalCompatibilite> compatibilites; 

        //constructeur de base
        public Animal()
        {
            identifiant = "";
            nom = "";
            type = "";
            sexe = ' ';
            sterilise = false;
            date_sterilisation = DateTime.MinValue;
            date_naissance = DateTime.MinValue;
            date_deces = DateTime.MinValue;
            race = "";
            particularites = "";
            couleurs = new List<string>();
            compatibilites = new List<AnimalCompatibilite>();
        }

        // construct param pour créer un nouvel animal (identifiant auto-généré)
        public Animal (string nom, string type, char sexe, DateTime dateNaissance)
        {
            this.identifiant = GenererIdentifiant();
            this.Nom = nom;
            this.Type = type;
            this.Sexe = sexe;
            this.DateNaissance = dateNaissance;
            this.sterilise = false;
            this.date_sterilisation= DateTime.MinValue;
            this.date_deces = DateTime.MinValue;
            this.race = "";
            this.particularites = "";
            this.couleurs = new List<string>();
            this.compatibilites = new List<AnimalCompatibilite>();
        }

        // construct param pour reconstruire un animal depuis la BD (avec identifiant existant)
        public Animal (string identifiant, string nom, string type, char sexe, DateTime dateNaissance)
        {
            this.identifiant = identifiant;
            this.Nom = nom;
            this.Type = type;
            this.Sexe = sexe;
            this.DateNaissance = dateNaissance;
            this.sterilise = false;
            this.date_sterilisation= DateTime.MinValue;
            this.date_deces = DateTime.MinValue;
            this.race = "";
            this.particularites = "";
            this.couleurs = new List<string>();
            this.compatibilites = new List<AnimalCompatibilite>();
        }
        // construct copie
        public Animal(Animal animal)
        {
            identifiant = animal.identifiant;
            nom = animal.nom;
            type = animal.type;
            sexe= animal.sexe;
            sterilise= animal.sterilise;
            date_sterilisation = animal.date_sterilisation;
            date_naissance= animal.date_naissance;
            date_deces= animal.date_deces;
            race = animal.race;
            particularites = animal.particularites;
            couleurs = new List<string>(animal.couleurs); 
            compatibilites = new List<AnimalCompatibilite>(animal.compatibilites);
        }
        //Méthode statique pour générer un identifiant avec séquence automatique (réinitialise chaque jour)
        public static string GenererIdentifiant()
        {
            string dateStr = DateTime.Now.ToString("yyMMdd");

            // si on change de jour, on réinitialise la séquence à 1
            if (dateStr != derniereDate)
            {
                prochainNumeroSequence = 1;
                derniereDate = dateStr;
            }

            if (prochainNumeroSequence > 99999)
                throw new InvalidOperationException("La séquence a atteint sa limite (99999) pour aujourd'hui.");

            string sequenceStr = prochainNumeroSequence.ToString("D5");
            prochainNumeroSequence++; // incrémenter pour le prochain
            return dateStr + sequenceStr;
        }

        //Méthode pour réinitialiser la séquence (utile pour les tests ou nouveau jour)
        public static void ReinitialiserSequence(int valeur = 1)
        {
            if (valeur < 1 || valeur > 99999)
                throw new ArgumentException("La séquence doit être entre 1 et 99999.", nameof(valeur));
            prochainNumeroSequence = valeur;
        }

        //Méthode pour initialiser la séquence depuis le dernier ID en BD (appelée au démarrage)
        public static void InitialiserDepuisBD(string dernierID)
        {
            if (string.IsNullOrEmpty(dernierID) || dernierID.Length != 11)
                return; // aucun animal en BD ou format invalide, on commence à 1

            // extrait la date et le numéro de séquence du dernier ID
            string dateIDStr = dernierID.Substring(0, 6); // ex: "260126"
            int dernierNumero = int.Parse(dernierID.Substring(6)); // ex: 5

            // compare avec la date d'aujourd'hui
            string dateAujourdhui = DateTime.Now.ToString("yyMMdd");

            if (dateIDStr == dateAujourdhui)
            {
                // même jour → on continue la séquence
                prochainNumeroSequence = dernierNumero + 1;
                derniereDate = dateAujourdhui;
            }
            // sinon (jour différent) → on laisse la séquence à 1 (valeur par défaut)
        }

        //Propriétés avec validation
        // Identifiant: 11 chiffres format yymmddXXXXX (généré automatiquement)
        public string Identifiant
        {
            get { return this.identifiant; }
            private set { this.identifiant = value; }
        }

        // Nom: min 2 caractères
        public string Nom
        {
            get { return this.nom; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le nom ne peut pas être vide.", nameof(value));
                if (value.Trim().Length < 2)
                    throw new ArgumentException("Le nom doit contenir au moins 2 caractères.", nameof(value));
                this.nom = value.Trim();
            }
        }

        // Type: chat ou chien uniquement
        public string Type
        {
            get { return this.type; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le type ne peut pas être vide.", nameof(value));
                string v = value.ToLower().Trim();
                if (v != "chien" && v != "chat")
                    throw new ArgumentException("Le type doit être 'chien' ou 'chat'.", nameof(value));
                this.type = v;
            }
        }

        // Sexe: M ou F
        public char Sexe
        {
            get { return this.sexe; }
            set
            {
                char s = char.ToUpper(value);
                if (s != 'M' && s != 'F')
                    throw new ArgumentException("Le sexe doit être 'M' ou 'F'.", nameof(value));
                this.sexe = s;
            }
        }

        public bool Sterilise
        {
            get { return this.sterilise; }
            set { this.sterilise = value; }
        }

        // Date stérilisation: obligatoire si stérilisé, sinon null
        public DateTime DateSterilisation
        {
            get { return this.date_sterilisation; }
            set
            {
                if (!sterilise && value != DateTime.MinValue)
                    throw new ArgumentException("Si l'animal n'est pas stérilisé, la date de stérilisation doit être nulle.", nameof(value));
                if (sterilise && value == DateTime.MinValue)
                    throw new ArgumentException("Si l'animal est stérilisé, la date de stérilisation doit être fournie.", nameof(value));
                if (value != DateTime.MinValue && date_naissance != DateTime.MinValue && value < date_naissance)
                    throw new ArgumentException("La date de stérilisation doit être supérieure ou égale à la date de naissance.", nameof(value));
                this.date_sterilisation = value;
            }
        }

        // Date naissance: obligatoire, <= aujourd'hui
        public DateTime DateNaissance
        {
            get { return this.date_naissance; }
            set
            {
                if (value == DateTime.MinValue)
                    throw new ArgumentException("La date de naissance ne peut pas être vide.", nameof(value));
                if (value > DateTime.Now)
                    throw new ArgumentException("La date de naissance doit être inférieure ou égale à aujourd'hui.", nameof(value));
                this.date_naissance = value;
            }
        }

        public DateTime DateDeces
        {
            get { return this.date_deces; }
            set
            {
                if (value != DateTime.MinValue && date_naissance != DateTime.MinValue && value < date_naissance)
                    throw new ArgumentException("La date de décès doit être supérieure ou égale à la date de naissance.", nameof(value));
                this.date_deces = value;
            }
        }

        public string Race
        {
            get { return this.race; }
            set { this.race = value; }
        }

        public string Particularites
        {
            get { return this.particularites; }
            set { this.particularites = value; }
        }

        //Ajoute une couleure de l animal
        public void AjouterCouleur(string couleur)
        {
            if (string.IsNullOrWhiteSpace(couleur))
                throw new ArgumentException("La couleur ne peut pas être vide.", nameof(couleur));

            couleur = couleur.Trim().ToLower();

            if (!couleurs.Contains(couleur))
                couleurs.Add(couleur);
        }
        //supprime une couleure de l animal
        public void SupprimerCouleur(string couleur)
        {
            if (string.IsNullOrWhiteSpace(couleur))
                throw new ArgumentException("La couleur ne peut pas être vide.", nameof(couleur));

            couleur = couleur.Trim().ToLower();
            couleurs.Remove(couleur);
        }
        //retourne une copie de la liste des couleurs "pour protéger la liste "
        public List<string> GetCouleurs()
        {
            return new List<string>(couleurs); 
        }

        //Ajout une compatibilité a l'animal
        public void AjouterCompatibilite(AnimalCompatibilite comp)
        {
            if (comp == null)
                throw new ArgumentException("La compatibilité ne peut pas être nulle.", nameof(comp));

            compatibilites.Add(comp);
        }
        //Supprime une Compatibilite de l animal
        public void SupprimerCompatibilite(int compType)
        {
            compatibilites.RemoveAll(c => c.CompType == compType);
        }
        //Retourne une copie de la liste compatibilités
        public List<AnimalCompatibilite> GetCompatibilites()
        {
            return new List<AnimalCompatibilite>(compatibilites); 
        }

        //calcul l'age de l aniaml
        public int CalculerAge()
        {
            DateTime dateRef; 
            if (date_deces != DateTime.MinValue)
            {
                dateRef = date_deces;
            }
            else 
            {
                dateRef = DateTime.Now;
            }
            return dateRef.Year - date_naissance.Year;
        }
        //Vérifie l'animal est vivant?!
        public bool EstVivant()
        {
            return date_deces == DateTime.MinValue;    // return un oui ou un non true false        
        }   
        // pour afficher l animal
        public override string ToString()
        {
            return identifiant + ", " + nom + ", " + type + ", " + sexe + date_naissance.ToShortDateString();
        }
    }
}
