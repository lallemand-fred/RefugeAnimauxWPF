using System;
using System.Windows;
using System.Windows.Controls;
using RefugeAnimaux.classeMetier;

namespace RefugeAnimaux.coucheVue
{
    public partial class FormulaireAnimal : Window
    {
        // l'animal d'origine (null = mode ajout)
        private Animal animalOriginal;

        // l'animal cree ou modifie, recup par MainWindow apres fermeture
        public Animal ResultAnimal { get; private set; }

        // mode ajout
        public FormulaireAnimal()
        {
            InitializeComponent();
            animalOriginal = null;
            txtIdentifiant.Text = "(auto-genere)";
            this.Title = "Ajouter un animal";
        }

        // mode modification
        public FormulaireAnimal(Animal animal)
        {
            InitializeComponent();
            animalOriginal = animal;
            this.Title = "Modifier un animal";

            // on remplit les champs avec les valeurs de l'animal
            txtIdentifiant.Text = animal.Identifiant;
            txtNom.Text = animal.Nom;

            // selectionner le bon item dans le combobox type
            foreach (ComboBoxItem item in cmbType.Items)
            {
                if (item.Content.ToString() == animal.Type)
                {
                    cmbType.SelectedItem = item;
                    break;
                }
            }

            // selectionner le bon item dans le combobox sexe
            foreach (ComboBoxItem item in cmbSexe.Items)
            {
                if (item.Content.ToString() == animal.Sexe.ToString())
                {
                    cmbSexe.SelectedItem = item;
                    break;
                }
            }

            dpDateNaissance.SelectedDate = animal.DateNaissance;
        }

        private void BtnEnregistrer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // recup les valeurs des champs
                string nom = txtNom.Text.Trim();
                if (string.IsNullOrWhiteSpace(nom))
                {
                    MessageBox.Show("Le nom est obligatoire.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtNom.Focus();
                    return;
                }

                if (cmbType.SelectedItem == null)
                {
                    MessageBox.Show("Le type est obligatoire.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                string type = ((ComboBoxItem)cmbType.SelectedItem).Content.ToString();

                if (cmbSexe.SelectedItem == null)
                {
                    MessageBox.Show("Le sexe est obligatoire.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                char sexe = ((ComboBoxItem)cmbSexe.SelectedItem).Content.ToString()[0];

                if (dpDateNaissance.SelectedDate == null)
                {
                    MessageBox.Show("La date de naissance est obligatoire.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                DateTime dateNaissance = dpDateNaissance.SelectedDate.Value;

                if (animalOriginal == null)
                {
                    // mode ajout : on cree un nouvel animal (ID auto-genere)
                    ResultAnimal = new Animal(nom, type, sexe, dateNaissance);
                }
                else
                {
                    // mode modif : on reconstruit avec le meme identifiant
                    ResultAnimal = new Animal(animalOriginal.Identifiant, nom, type, sexe, dateNaissance);

                    // on recopie les champs qu'on modifie pas ici
                    ResultAnimal.Sterilise = animalOriginal.Sterilise;
                    if (animalOriginal.Sterilise)
                        ResultAnimal.DateSterilisation = animalOriginal.DateSterilisation;
                    ResultAnimal.Race = animalOriginal.Race;
                    ResultAnimal.Particularites = animalOriginal.Particularites;
                    if (animalOriginal.DateDeces != DateTime.MinValue)
                        ResultAnimal.DateDeces = animalOriginal.DateDeces;
                }

                this.DialogResult = true;
            }
            catch (ArgumentException ex)
            {
                // les validations de la classe Animal attrapees ici
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
