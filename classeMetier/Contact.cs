using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RefugeAnimaux.classeMetier
{
    public class Contact
    {
        private int contact_identifiant;
        private string nom;
        private string prenom;
        private string registre_national;
        private Adresse adresse_contact;
        private string gsm;
        private string telephone;
        private string email;
        private List<Role> roles; // Liste des rôles du contact

        //construct de base
        public Contact()
        {
            contact_identifiant = 0;
            nom = "";
            prenom = "";
            registre_national = "";
            adresse_contact = null;
            gsm = "";
            telephone = "";
            email = "";
            roles = new List<Role>();
        }
        //construct param
        public Contact(int id, string nom, string prenom, string registre, string gsm, string telephone, string email, Adresse adresse)
        {
            // Validation Nom min 2 caractères
            if (string.IsNullOrWhiteSpace(nom))
                throw new ArgumentException("Le nom ne peut pas être vide.", nameof(nom));
            if (nom.Trim().Length < 2)
                throw new ArgumentException("Le nom doit avoir min 2 caractères.", nameof(nom));

            // Validation Prenom min 2 caractères
            if (string.IsNullOrWhiteSpace(prenom))
                throw new ArgumentException("Le prénom ne peut pas être vide.", nameof(prenom));
            if (prenom.Trim().Length < 2)
                throw new ArgumentException("Le prénom doit avoir min 2 caractères.", nameof(prenom));

            this.contact_identifiant = id;
            this.nom = nom.Trim();
            this.prenom = prenom.Trim();
            this.registre_national = registre;
            this.gsm = gsm;
            this.telephone = telephone;
            this.email = email;
            this.adresse_contact= adresse;
            this.roles = new List<Role>();

            // Vérifier qu'au moins un moyen de contact est fourni
            VerifierContactMoyen();
        }

        //construct copie
        public Contact(Contact contact)
        {
            contact_identifiant= contact.contact_identifiant;
            nom = contact.nom;
            prenom = contact.prenom;
            registre_national = contact.registre_national;
            adresse_contact = new Adresse(contact.adresse_contact);
            gsm = contact.gsm;
            telephone = contact.telephone;
            email = contact.email;
            roles = new List<Role>(contact.roles); // Copie de la liste
        }

        //Propriétés
        // Id: auto-généré, ne peut pas être modifié
        public int Id
        {
            get { return this.contact_identifiant; }
            private set { this.contact_identifiant = value; }
        }

        public string Nom
        {
            get { return this.nom; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le nom ne peut pas être vide.", nameof(value));
                if (value.Trim().Length < 2)
                    throw new ArgumentException("Le nom doit avoir min 2 caractères.", nameof(value));
                this.nom = value.Trim();
            }
        }

        public string Prenom
        {
            get { return this.prenom; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Le prénom ne peut pas être vide.", nameof(value));
                if (value.Trim().Length < 2)
                    throw new ArgumentException("Le prénom doit avoir min 2 caractères.", nameof(value));
                this.prenom = value.Trim();
            }
        }

        // Registre national: format yy.mm.dd-999.99
        public string RegistreNational
        {
            get { return this.registre_national; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // Vérifier le format de base
                    string pattern = @"^\d{2}\.\d{2}\.\d{2}-\d{3}\.\d{2}$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                        throw new ArgumentException("Le registre national doit avoir le format yy.mm.dd-999.99", nameof(value));

                    // Valider la date de naissance
                    string[] parties = value.Split('-');
                    string[] dateParts = parties[0].Split('.');
                    int yy = int.Parse(dateParts[0]);
                    int mm = int.Parse(dateParts[1]);
                    int dd = int.Parse(dateParts[2]);

                    // Vérifier que le mois est valide (01-12)
                    if (mm < 1 || mm > 12)
                        throw new ArgumentException("Le mois dans le registre national doit être entre 01 et 12.", nameof(value));

                    // Vérifier que le jour est valide (01-31)
                    if (dd < 1 || dd > 31)
                        throw new ArgumentException("Le jour dans le registre national doit être entre 01 et 31.", nameof(value));

                    // Vérifier les jours selon le mois
                    int[] joursParMois = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                    if (dd > joursParMois[mm - 1])
                        throw new ArgumentException($"Le mois {mm:D2} ne peut pas avoir {dd} jours.", nameof(value));

                    // déterminer le siècle et calculer l'année complète
                    int anneeComplete;
                    if (yy <= 26)
                        anneeComplete = 2000 + yy;
                    else
                        anneeComplete = 1900 + yy;

                    // calculer l'âge
                    int anneeActuelle = 2026;
                    int age = anneeActuelle - anneeComplete;

                    // validation âge minimum 12 ans
                    if (age < 12)
                        throw new ArgumentException($"La personne doit avoir au moins 12 ans. Age calculé: {age} ans.", nameof(value));

                    // validation âge maximum 90 ans
                    if (age > 90)
                        throw new ArgumentException($"Age invalide ({age} ans). L'âge maximum autorisé est 90 ans.", nameof(value));
                }
                this.registre_national = value;
            }
        }

        public Adresse Adresse
        {
            get { return this.adresse_contact; }
            set { this.adresse_contact = value; }
        }

        public string GSM
        {
            get { return this.gsm; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    // validation format GSM belge: 04 suivi de 8 chiffres
                    string pattern = @"^04\d{8}$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                        throw new ArgumentException("Le GSM doit avoir le format 04XXXXXXXX (10 chiffres commençant par 04)", nameof(value));
                }
                this.gsm = value;
                VerifierContactMoyen();
            }
        }

        public string Telephone
        {
            get { return this.telephone; }
            set
            {
                this.telephone = value;
                VerifierContactMoyen();
            }
        }

        // Email: format xxx@xxx.xxx
        public string Email
        {
            get { return this.email; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                    if (!System.Text.RegularExpressions.Regex.IsMatch(value, pattern))
                        throw new ArgumentException("L'email doit avoir un format valide (xxx@xxx.xxx)", nameof(value));
                }
                this.email = value;
                VerifierContactMoyen();
            }
        }

        //Méthode privée pour vérifier que GSM et Email sont fournis
        private void VerifierContactMoyen()
        {
            if (string.IsNullOrWhiteSpace(gsm))
                throw new ArgumentException("Le GSM est obligatoire.");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("L'email est obligatoire.");
        }

        //Gestion des rôles
        public void AjouterRole(Role role)
        {
            if (role == null)
                throw new ArgumentException("Le rôle ne peut pas être nul.", nameof(role));

            roles.Add(role);
        }

        public void SupprimerRole(int roleId)
        {
            roles.RemoveAll(r => r.RolIdentifiant == roleId);
        }

        public List<Role> GetRoles()
        {
            return new List<Role>(roles); // Retourne une copie pour protéger la liste interne
        }

        public override string ToString()
        {
            return contact_identifiant + ", " + nom + ", " + prenom + ", " + gsm + ", " + email;
        }
    }
}
