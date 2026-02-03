using System;
using System.Text.RegularExpressions;
using System.Windows;
using RefugeAnimaux.classeMetier;

namespace RefugeAnimaux.coucheVue
{
    public partial class FormulaireContact : Window
    {
        // le contact d'origine (null = mode ajout)
        private Contact contactOriginal;

        // le contact cree ou modifie, recup par MainWindow apres fermeture
        public Contact ResultContact { get; private set; }

        // mode ajout
        public FormulaireContact()
        {
            InitializeComponent();
            contactOriginal = null;
            txtId.Text = "(auto-genere)";
            this.Title = "Ajouter un contact";
        }

        // mode modification
        public FormulaireContact(Contact contact)
        {
            InitializeComponent();
            contactOriginal = contact;
            this.Title = "Modifier un contact";

            // on remplit les champs avec les valeurs du contact
            txtId.Text = contact.Id.ToString();
            txtNom.Text = contact.Nom;
            txtPrenom.Text = contact.Prenom;
            txtRegistreNational.Text = contact.RegistreNational;
            txtGSM.Text = contact.GSM;
            txtTelephone.Text = contact.Telephone;
            txtEmail.Text = contact.Email;

            // adresse si elle existe
            if (contact.Adresse != null)
            {
                txtRue.Text = contact.Adresse.Rue;
                txtCodePostal.Text = contact.Adresse.Cp;
                txtLocalite.Text = contact.Adresse.Localite;
            }
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // validation nom/prenom avant de creer l'objet
                string nom = txtNom.Text.Trim();
                if (string.IsNullOrWhiteSpace(nom) || nom.Length < 2)
                {
                    MessageBox.Show("Le nom est obligatoire (min 2 caracteres).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNom.Focus();
                    return;
                }

                string prenom = txtPrenom.Text.Trim();
                if (string.IsNullOrWhiteSpace(prenom) || prenom.Length < 2)
                {
                    MessageBox.Show("Le prenom est obligatoire (min 2 caracteres).", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPrenom.Focus();
                    return;
                }

                // validation registre national si rempli
                string registreNational = txtRegistreNational.Text.Trim();
                if (!string.IsNullOrWhiteSpace(registreNational))
                {
                    // check le format AA.MM.JJ-XXX.YY
                    if (!Regex.IsMatch(registreNational, @"^[0-9]{2}\.[0-9]{2}\.[0-9]{2}-[0-9]{3}\.[0-9]{2}$"))
                    {
                        MessageBox.Show("Format incorrect. Le registre national doit etre au format AA.MM.JJ-XXX.YY (ex: 81.10.01-654.32).",
                            "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtRegistreNational.Focus();
                        return;
                    }

                    // j'extrais les parties du registre
                    int aa = int.Parse(registreNational.Substring(0, 2));
                    int mm = int.Parse(registreNational.Substring(3, 2));
                    int jj = int.Parse(registreNational.Substring(6, 2));

                    // mois valide?
                    if (mm < 1 || mm > 12)
                    {
                        MessageBox.Show("Le mois doit etre entre 01 et 12.",
                            "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtRegistreNational.Focus();
                        return;
                    }

                    // jour valide?
                    if (jj < 1 || jj > 31)
                    {
                        MessageBox.Show("Le jour doit etre entre 01 et 31.",
                            "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtRegistreNational.Focus();
                        return;
                    }

                    // calcul annee de naissance : 00-26 -> 2000+, 27-99 -> 1900+
                    int anneeNaissance = (aa <= 26) ? 2000 + aa : 1900 + aa;
                    int age = DateTime.Now.Year - anneeNaissance;

                    if (age < 12)
                    {
                        MessageBox.Show("Le contact doit avoir au moins 12 ans.",
                            "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtRegistreNational.Focus();
                        return;
                    }

                    if (age > 95)
                    {
                        MessageBox.Show("L'age ne peut pas depasser 95 ans.",
                            "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtRegistreNational.Focus();
                        return;
                    }
                }

                // validation email si rempli
                string email = txtEmail.Text.Trim();
                if (!string.IsNullOrWhiteSpace(email))
                {
                    if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                    {
                        MessageBox.Show("Format d'email invalide.\n\nExemple valide : contact@example.com",
                            "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtEmail.Focus();
                        return;
                    }
                }

                // validation code postal si rempli : 4 chiffres obligatoires
                string codePostal = txtCodePostal.Text.Trim();
                if (!string.IsNullOrWhiteSpace(codePostal))
                {
                    if (!Regex.IsMatch(codePostal, @"^[0-9]{4}$"))
                    {
                        MessageBox.Show("Le code postal doit contenir exactement 4 chiffres (ex: 4800).",
                            "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                        txtCodePostal.Focus();
                        return;
                    }
                }

                // cree l'adresse
                Adresse adresse = new Adresse(
                    txtRue.Text.Trim(),
                    txtCodePostal.Text.Trim(),
                    txtLocalite.Text.Trim()
                );

                // recup l'id selon le mode
                int id = 0;
                if (contactOriginal != null)
                    id = contactOriginal.Id;

                // cree le contact (les validations de la classe Contact vont jouer)
                ResultContact = new Contact(
                    id,
                    nom,
                    prenom,
                    txtRegistreNational.Text.Trim(),
                    txtGSM.Text.Trim(),
                    txtTelephone.Text.Trim(),
                    txtEmail.Text.Trim(),
                    adresse
                );

                this.DialogResult = true;
            }
            catch (ArgumentException ex)
            {
                // les validations de la classe Contact/Adresse attrapees ici
                MessageBox.Show(ex.Message, "Erreur de validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
