using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using RefugeAnimaux.classeMetier;
using RefugeAnimaux.coucheAccesBD;

namespace RefugeAnimaux.coucheModeleVue
{
    /// <summary>
    /// Vue-Modele principale - fait le lien entre AccesBD et l'interface WPF
    /// </summary>
    public class Vue_Modele : INotifyPropertyChanged
    {
        // ============================================================
        // EVENEMENT INotifyPropertyChanged
        // ============================================================

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// notifie la vue qu'une propriete a change
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // ============================================================
        // PROPRIETES PRIVEES
        // ============================================================

        private AccesBD accesBD;

        // collections observables
        private ObservableCollection<Animal> listeAnimaux;
        private ObservableCollection<Contact> listeContacts;

        // elements selectionnes
        private Animal animalSelectionne;
        private Contact contactSelectionne;

        // ============================================================
        // CONSTRUCTEUR
        // ============================================================

        public Vue_Modele()
        {
            // init la couche d'acces aux donnees
            accesBD = new AccesBD();

            // init la sequence d'identifiants depuis la BD
            // sinon ca repart a 1 a chaque demarrage et ca fait des doublons
            string dernierID = accesBD.ObtenirDernierIdentifiantAnimal();
            Animal.InitialiserDepuisBD(dernierID);

            // init les collections observables
            listeAnimaux = new ObservableCollection<Animal>();
            listeContacts = new ObservableCollection<Contact>();

            // charge les donnees au demarrage
            ChargerAnimaux();
            ChargerContacts();
        }

        // ============================================================
        // PROPRIETES PUBLIQUES (avec notification)
        // ============================================================

        /// <summary>
        /// Liste observable des animaux du refuge
        /// </summary>
        public ObservableCollection<Animal> ListeAnimaux
        {
            get { return listeAnimaux; }
            set
            {
                if (listeAnimaux != value)
                {
                    listeAnimaux = value;
                    OnPropertyChanged(nameof(ListeAnimaux));
                }
            }
        }

        /// <summary>
        /// Liste observable des contacts
        /// </summary>
        public ObservableCollection<Contact> ListeContacts
        {
            get { return listeContacts; }
            set
            {
                if (listeContacts != value)
                {
                    listeContacts = value;
                    OnPropertyChanged(nameof(ListeContacts));
                }
            }
        }

        /// <summary>
        /// Animal actuellement selectionne dans l'interface
        /// </summary>
        public Animal AnimalSelectionne
        {
            get { return animalSelectionne; }
            set
            {
                if (animalSelectionne != value)
                {
                    animalSelectionne = value;
                    OnPropertyChanged(nameof(AnimalSelectionne));
                }
            }
        }

        /// <summary>
        /// Contact actuellement selectionne dans l'interface
        /// </summary>
        public Contact ContactSelectionne
        {
            get { return contactSelectionne; }
            set
            {
                if (contactSelectionne != value)
                {
                    contactSelectionne = value;
                    OnPropertyChanged(nameof(ContactSelectionne));
                }
            }
        }

        // ============================================================
        // METHODES DE CHARGEMENT
        // ============================================================

        /// <summary>
        /// Charge la liste des animaux depuis la BD
        /// </summary>
        public void ChargerAnimaux()
        {
            try
            {
                listeAnimaux.Clear();

                // recup les animaux depuis AccesBD
                var animaux = accesBD.ListeAnimaux();

                foreach (var animal in animaux)
                {
                    listeAnimaux.Add(animal);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur chargement animaux: {ex.Message}", ex);
            }
        }

        // retourne que les animaux presents au refuge (pour le formulaire de sortie)
        public ObservableCollection<Animal> GetAnimauxPresents()
        {
            try
            {
                var presents = new ObservableCollection<Animal>();
                var animaux = accesBD.ListeAnimauxPresents();
                foreach (var animal in animaux)
                {
                    presents.Add(animal);
                }
                return presents;
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur chargement animaux presents: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Charge la liste des contacts depuis la BD
        /// </summary>
        public void ChargerContacts()
        {
            try
            {
                listeContacts.Clear();

                // recup les contacts depuis AccesBD
                var contacts = accesBD.ListeContacts();

                foreach (var contact in contacts)
                {
                    listeContacts.Add(contact);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur chargement contacts: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES D'AJOUT
        // ============================================================

        /// <summary>
        /// Ajoute un nouvel animal dans la BD et la liste
        /// </summary>
        public void AjouterAnimal(Animal animal)
        {
            try
            {
                int resultat = accesBD.AjouterAnimal(animal);

                if (resultat > 0)
                {
                    listeAnimaux.Add(animal);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur ajout animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Ajoute un nouveau contact dans la BD et la liste
        /// </summary>
        public void AjouterContact(Contact contact)
        {
            try
            {
                // AjouterContact retourne l'ID auto-genere
                int idContact = accesBD.AjouterContact(contact);

                if (idContact > 0)
                {
                    // recharge la liste pour avoir le bon ID
                    ChargerContacts();
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur ajout contact: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enregistre l'arrivee d'un animal au refuge
        /// Workflow qui touche plusieurs tables : animal, ani_entree, contact
        /// </summary>
        public void EnregistrerArrivee(Animal animal, bool estNouvelAnimal, AnimalEntree entree, Contact nouveauContact)
        {
            try
            {
                // 1. si nouveau contact, on l'ajoute d'abord pour avoir son ID
                if (nouveauContact != null)
                {
                    int idContact = accesBD.AjouterContact(nouveauContact);
                    // met a jour le ContactId de l'entree avec le nouvel ID
                    entree.ContactId = idContact;
                }

                // 2. si nouvel animal, on l'ajoute
                if (estNouvelAnimal)
                {
                    accesBD.AjouterAnimal(animal);
                }

                // 3. on ajoute l'entree
                accesBD.AjouterEntreeAnimal(entree);

                // 4. rafraichit les listes
                ChargerAnimaux();
                ChargerContacts();
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur enregistrement arrivee: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Enregistre la sortie d'un animal du refuge
        /// AccesBD gere deja la mise a jour de date_deces si raison='deces_animal'
        /// </summary>
        public void EnregistrerSortie(AnimalSortie sortie)
        {
            try
            {
                // AjouterSortieAnimal gere tout : validation presence, ajout sortie, mise a jour date_deces
                accesBD.AjouterSortieAnimal(sortie);

                // rafraichit la liste des animaux
                ChargerAnimaux();
            }
            catch (ExceptionAccesBD ex)
            {
                // on affiche le message + les details pour savoir ce qui plante
                string msg = ex.Message;
                string details = ex.GetDetails();
                if (!string.IsNullOrEmpty(details))
                    msg += " - " + details;
                throw new Exception(msg, ex);
            }
        }

        /// <summary>
        /// Ajoute une nouvelle demande d'adoption
        /// </summary>
        public void AjouterAdoption(Adoption adoption)
        {
            try
            {
                accesBD.AjouterAdoption(adoption);
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur ajout adoption: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Modifie le statut d'une adoption existante
        /// </summary>
        public void ModifierStatutAdoption(Adoption adoption)
        {
            try
            {
                accesBD.UpdateStatutAdoption(adoption.AnimalId, adoption.DateDemande, adoption.Statut);
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur modification statut adoption: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retourne la liste de toutes les adoptions
        /// </summary>
        public List<Adoption> ObtenirListeAdoptions()
        {
            try
            {
                var adoptions = accesBD.ListeAdoptions();

                // remplit les refs typees depuis les listes en memoire
                foreach (var adoption in adoptions)
                {
                    // cherche l'animal correspondant
                    foreach (var animal in listeAnimaux)
                    {
                        if (animal.Identifiant == adoption.AnimalId)
                        {
                            adoption.Animal = animal;
                            break;
                        }
                    }

                    // cherche le contact correspondant
                    foreach (var contact in listeContacts)
                    {
                        if (contact.Id == adoption.ContactId)
                        {
                            adoption.Contact = contact;
                            break;
                        }
                    }
                }

                return adoptions;
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur lecture adoptions: {ex.Message}", ex);
            }
        }

        // supprime une adoption manuellement
        public void SupprimerAdoption(Adoption adoption)
        {
            try
            {
                accesBD.SupprimerAdoption(adoption.AnimalId, adoption.DateDemande);
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur suppression adoption: {ex.Message}", ex);
            }
        }

        // nettoie les adoptions orphelines (animal ou contact supprime)
        public int NettoyerAdoptionsOrphelines()
        {
            try
            {
                return accesBD.NettoyerAdoptionsOrphelines();
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur nettoyage adoptions: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES DE MODIFICATION
        // ============================================================

        /// <summary>
        /// Modifie un animal existant dans la BD
        /// </summary>
        public void ModifierAnimal(Animal animal)
        {
            try
            {
                int resultat = accesBD.ModifierAnimal(animal);

                if (resultat > 0)
                {
                    // notifie le changement pour rafraichir la vue
                    OnPropertyChanged(nameof(ListeAnimaux));
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur modification animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Modifie un contact existant dans la BD
        /// </summary>
        public void ModifierContact(Contact contact)
        {
            try
            {
                int resultat = accesBD.ModifierContact(contact);

                if (resultat > 0)
                {
                    OnPropertyChanged(nameof(ListeContacts));
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur modification contact: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES DE SUPPRESSION
        // ============================================================

        /// <summary>
        /// Supprime un animal de la BD et de la liste
        /// </summary>
        public void SupprimerAnimal(Animal animal)
        {
            try
            {
                int resultat = accesBD.SupprimerAnimal(animal.Identifiant);

                if (resultat > 0)
                {
                    listeAnimaux.Remove(animal);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur suppression animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Supprime un contact de la BD et de la liste
        /// </summary>
        public void SupprimerContact(Contact contact)
        {
            try
            {
                int resultat = accesBD.SupprimerContact(contact.Id);

                if (resultat > 0)
                {
                    listeContacts.Remove(contact);
                }
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur suppression contact: {ex.Message}", ex);
            }
        }

        // ============================================================
        // METHODES DE RECHERCHE
        // ============================================================

        /// <summary>
        /// Recherche un animal par son identifiant (11 chiffres)
        /// </summary>
        public Animal RechercherAnimal(string identifiant)
        {
            try
            {
                return accesBD.ObtenirAnimal(identifiant);
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur recherche animal: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Recherche un contact par son ID
        /// </summary>
        public Contact RechercherContact(int id)
        {
            try
            {
                return accesBD.ObtenirContact(id);
            }
            catch (ExceptionAccesBD ex)
            {
                throw new Exception($"Erreur recherche contact: {ex.Message}", ex);
            }
        }
    }
}
