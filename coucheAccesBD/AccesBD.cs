using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using RefugeAnimaux.classeMetier;

namespace RefugeAnimaux.coucheAccesBD
{
    public class AccesBD : IDisposable
    {
        private NpgsqlConnection sqlConn;
        //constructeur connexion PostgreSQL
        public AccesBD() 
        {
            try
            {
                string connectionString = "Server=localhost;Port=5432;Database=refuge_animaux;" + "User Id=postgres;Password=P@ssword;";
                sqlConn = new NpgsqlConnection(connectionString);

                
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Connexion à la base de données", ex.Message);
            }
        }

        public void Dispose()
        { 
            try{
                if(sqlConn != null)
                        {
                    if (sqlConn.State != System.Data.ConnectionState.Closed)
                        sqlConn.Close();
                } 
            }
            catch
            {
            }
        }
        //Lister tous les animaux
        public List<Animal> ListeAnimaux()
        {
            List<Animal> animaux = new List<Animal>();
            try
            {
                sqlConn.Open();
                // on recup tous les animaux vivants avec leur statut calcule
                // si nb sorties >= nb entrees (et au moins 1 sortie) -> statut selon derniere sortie
                // sinon -> au refuge (plus d'entrees que de sorties = animal present)
                string query = @"
                    SELECT a.identifiant, a.nom, a.type, a.sexe, a.date_naissance,
                        CASE
                            WHEN (SELECT COUNT(*) FROM ANI_SORTIE WHERE ani_identifiant = a.identifiant) > 0
                                 AND (SELECT COUNT(*) FROM ANI_SORTIE WHERE ani_identifiant = a.identifiant)
                                     >= (SELECT COUNT(*) FROM ANI_ENTREE WHERE ani_identifiant = a.identifiant)
                            THEN
                                (SELECT CASE raison
                                    WHEN 'adoption' THEN 'Adopte'
                                    WHEN 'famille_accueil' THEN 'Famille accueil'
                                    WHEN 'retour_proprietaire' THEN 'Retour proprietaire'
                                    WHEN 'deces_animal' THEN 'Decede'
                                    ELSE 'Sorti'
                                END
                                FROM ANI_SORTIE
                                WHERE ani_identifiant = a.identifiant
                                ORDER BY date_sortie DESC LIMIT 1)
                            ELSE 'Au refuge'
                        END AS statut
                    FROM ANIMAL a
                    WHERE a.date_deces IS NULL
                    ORDER BY a.nom";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string identifiant = reader.GetString(0).Trim();
                    string nom = reader.GetString(1);
                    string type = reader.GetString(2).Trim();
                    char sexe = reader.GetString(3).Trim()[0];
                    DateTime dateNaissance = reader.GetDateTime(4);
                    string statut = reader.GetString(5);

                    Animal animal = new Animal(identifiant, nom, type, sexe, dateNaissance);
                    animal.Statut = statut;
                    animaux.Add(animal);
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la lecture d'un animal", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return animaux;
        }

        // retourne les animaux vivants ET presents au refuge
        // present = plus d'entrees que de sorties (ou aucune sortie)
        public List<Animal> ListeAnimauxPresents()
        {
            List<Animal> animaux = new List<Animal>();
            try
            {
                sqlConn.Open();
                string query = @"
                    SELECT a.identifiant, a.nom, a.type, a.sexe, a.date_naissance
                    FROM ANIMAL a
                    WHERE a.date_deces IS NULL
                    AND NOT (
                        (SELECT COUNT(*) FROM ANI_SORTIE WHERE ani_identifiant = a.identifiant) > 0
                        AND (SELECT COUNT(*) FROM ANI_SORTIE WHERE ani_identifiant = a.identifiant)
                            >= (SELECT COUNT(*) FROM ANI_ENTREE WHERE ani_identifiant = a.identifiant)
                    )
                    ORDER BY a.nom";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string identifiant = reader.GetString(0).Trim();
                    string nom = reader.GetString(1);
                    string type = reader.GetString(2).Trim();
                    char sexe = reader.GetString(3).Trim()[0];
                    DateTime dateNaissance = reader.GetDateTime(4);

                    Animal animal = new Animal(identifiant, nom, type, sexe, dateNaissance);
                    animaux.Add(animal);
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la lecture des animaux presents", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return animaux;
        }

        //recup un animal par son ID
        public Animal ObtenirAnimal(string identifiant)
        {
            Animal animal = null;
            try
            {
                sqlConn.Open();
                string query = "SELECT identifiant, nom, type, sexe, date_naissance, sterilise, date_sterilisation, date_deces, race, particularites FROM ANIMAL WHERE identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", identifiant);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string id = reader.GetString(0);
                    string nom = reader.GetString(1);
                    string type = reader.GetString(2);
                    char sexe = reader.GetChar(3);
                    DateTime dateNaissance = reader.GetDateTime(4);

                    animal = new Animal(id, nom, type, sexe, dateNaissance);

                    // Propriétés supplémentaires
                    animal.Sterilise = reader.GetBoolean(5);
                    if (!reader.IsDBNull(6))
                        animal.DateSterilisation = reader.GetDateTime(6);
                    if (!reader.IsDBNull(7))
                        animal.DateDeces = reader.GetDateTime(7);
                    if (!reader.IsDBNull(8))
                        animal.Race = reader.GetString(8);
                    if (!reader.IsDBNull(9))
                        animal.Particularites = reader.GetString(9);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la lecture d'un animal", ex.Message);
            }
            return animal;
        }
        //recup un contact par son ID
        public Contact ObtenirContact(int contactId)
        {
            Contact contact = null;
            try
            {
                sqlConn.Open();
                string query = "SELECT contact_identifiant, nom, prenom, registre_national, gsm, telephone, email, " +
                               "(adresse_contact).rue, (adresse_contact).cp, (adresse_contact).localite " +
                               "FROM CONTACT WHERE contact_identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", contactId);

                NpgsqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string nom = reader.GetString(1);
                    string prenom = reader.GetString(2);
                    string registre = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    string gsm = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    string telephone = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    string email = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    string rue = reader.IsDBNull(7) ? "" : reader.GetString(7);
                    string cp = reader.IsDBNull(8) ? "" : reader.GetString(8);
                    string localite = reader.IsDBNull(9) ? "" : reader.GetString(9);

                    Adresse adresse = new Adresse(rue, cp, localite);
                    contact = new Contact(id, nom, prenom, registre, gsm, telephone, email, adresse);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la lecture d'un contact", ex.Message);
            }
            return contact;
        }

        // Modifie les coordonnées d'un contact
        public int ModifierContact(Contact contact)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = @"UPDATE CONTACT SET
                                nom = @nom,
                                prenom = @prenom,
                                registre_national = @registre,
                                gsm = @gsm,
                                telephone = @telephone,
                                email = @email,
                                adresse_contact = ROW(@rue, @cp, @localite)
                                WHERE contact_identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@id", contact.Id);
                cmd.Parameters.AddWithValue("@nom", contact.Nom);
                cmd.Parameters.AddWithValue("@prenom", contact.Prenom);
                cmd.Parameters.AddWithValue("@registre", contact.RegistreNational ?? "");
                cmd.Parameters.AddWithValue("@gsm", contact.GSM ?? "");
                cmd.Parameters.AddWithValue("@telephone", contact.Telephone ?? "");
                cmd.Parameters.AddWithValue("@email", contact.Email ?? "");
                cmd.Parameters.AddWithValue("@rue", contact.Adresse.Rue);
                cmd.Parameters.AddWithValue("@cp", contact.Adresse.Cp);
                cmd.Parameters.AddWithValue("@localite", contact.Adresse.Localite);

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la modification du contact", ex.Message);
            }
            return lignesAffectees;
        }

        // Supprime un contact (et toutes ses données liées en cascade)
        public int SupprimerContact(int contactId)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();

                // Supprimer dans l'ordre pour respecter les contraintes
                // 1. Rôles du contact
                string deleteRoles = "DELETE FROM PERSONNE_ROLE WHERE pers_identifiant = @id";
                NpgsqlCommand cmdRoles = new NpgsqlCommand(deleteRoles, sqlConn);
                cmdRoles.Parameters.AddWithValue("@id", contactId);
                cmdRoles.ExecuteNonQuery();
                cmdRoles.Dispose();

                // 2. Adoptions
                string deleteAdop = "DELETE FROM ADOPTION WHERE adop_contact = @id";
                NpgsqlCommand cmdAdop = new NpgsqlCommand(deleteAdop, sqlConn);
                cmdAdop.Parameters.AddWithValue("@id", contactId);
                cmdAdop.ExecuteNonQuery();
                cmdAdop.Dispose();

                // 3. Familles d'accueil
                string deleteFA = "DELETE FROM FAMILLE_ACCUEIL WHERE fa_contact = @id";
                NpgsqlCommand cmdFA = new NpgsqlCommand(deleteFA, sqlConn);
                cmdFA.Parameters.AddWithValue("@id", contactId);
                cmdFA.ExecuteNonQuery();
                cmdFA.Dispose();

                // 4. Entrées
                string deleteEntree = "DELETE FROM ANI_ENTREE WHERE entree_contact = @id";
                NpgsqlCommand cmdEntree = new NpgsqlCommand(deleteEntree, sqlConn);
                cmdEntree.Parameters.AddWithValue("@id", contactId);
                cmdEntree.ExecuteNonQuery();
                cmdEntree.Dispose();

                // 5. Sorties
                string deleteSortie = "DELETE FROM ANI_SORTIE WHERE sortie_contact = @id";
                NpgsqlCommand cmdSortie = new NpgsqlCommand(deleteSortie, sqlConn);
                cmdSortie.Parameters.AddWithValue("@id", contactId);
                cmdSortie.ExecuteNonQuery();
                cmdSortie.Dispose();

                // 6. Enfin le contact lui-même
                string deleteContact = "DELETE FROM CONTACT WHERE contact_identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(deleteContact, sqlConn);
                cmd.Parameters.AddWithValue("@id", contactId);
                lignesAffectees = cmd.ExecuteNonQuery();
                cmd.Dispose();

                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la suppression du contact", ex.Message);
            }
            return lignesAffectees;
        }
        //add un nouveau aniaml au refuge
        public int AjouterAnimal(Animal animal)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = "INSERT INTO ANIMAL (identifiant, nom, type, sexe, date_naissance, " +
                               "sterilise, date_sterilisation, race, particularites) " +
                               "VALUES (@id, @nom, @type, @sexe, @dateNaiss, @sterilise, @dateSteril, @desc, @part)";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animal.Identifiant);
                cmd.Parameters.AddWithValue("@nom", animal.Nom);
                cmd.Parameters.AddWithValue("@type", animal.Type);
                // faut envoyer le sexe en string, sinon Npgsql envoie le type "char" interne
                cmd.Parameters.AddWithValue("@sexe", animal.Sexe.ToString());
                cmd.Parameters.AddWithValue("@dateNaiss", animal.DateNaissance);
                cmd.Parameters.AddWithValue("@sterilise", animal.Sterilise);

                if (animal.DateSterilisation == DateTime.MinValue)
                {
                    cmd.Parameters.AddWithValue("@dateSteril", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@dateSteril", animal.DateSterilisation);
                }

                cmd.Parameters.AddWithValue("@desc", animal.Race ?? "");
                cmd.Parameters.AddWithValue("@part", animal.Particularites ?? "");

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de l'ajout d'un animal", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return lignesAffectees;
        }

        // Obtenir le dernier identifiant animal pour initialiser la séquence
        public string ObtenirDernierIdentifiantAnimal()
        {
            string dernierIdentifiant = null;
            try
            {
                sqlConn.Open();
                string query = "SELECT identifiant FROM ANIMAL ORDER BY identifiant DESC LIMIT 1";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                object result = cmd.ExecuteScalar();
                if (result != null)
                {
                    dernierIdentifiant = result.ToString();
                }

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la récupération du dernier identifiant animal", ex.Message);
            }
            return dernierIdentifiant;
        }

        // Modifier un animal
        public int ModifierAnimal(Animal animal)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = @"UPDATE ANIMAL SET
                                nom = @nom,
                                type = @type,
                                sexe = @sexe,
                                date_naissance = @dateNaiss,
                                race = @desc,
                                particularites = @part,
                                sterilise = @sterilise,
                                date_sterilisation = @dateSteril
                                WHERE identifiant = @id";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@id", animal.Identifiant);
                cmd.Parameters.AddWithValue("@nom", animal.Nom);
                cmd.Parameters.AddWithValue("@type", animal.Type);
                cmd.Parameters.AddWithValue("@sexe", animal.Sexe.ToString());
                cmd.Parameters.AddWithValue("@dateNaiss", animal.DateNaissance);
                cmd.Parameters.AddWithValue("@desc", animal.Race ?? "");
                cmd.Parameters.AddWithValue("@part", animal.Particularites ?? "");
                cmd.Parameters.AddWithValue("@sterilise", animal.Sterilise);

                if (animal.DateSterilisation == DateTime.MinValue)
                {
                    cmd.Parameters.AddWithValue("@dateSteril", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@dateSteril", animal.DateSterilisation);
                }

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la modification de l'animal", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return lignesAffectees;
        }

        // liste tous les contacts
        public List<Contact> ListeContacts()
        {
            List<Contact> contacts = new List<Contact>();

            try
            {
                sqlConn.Open();
                string query = "SELECT contact_identifiant, nom, prenom, registre_national, gsm, telephone, email, " +
                               "(adresse_contact).rue, (adresse_contact).cp, (adresse_contact).localite " +
                               "FROM CONTACT ORDER BY nom";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string nom = reader.GetString(1);
                    string prenom = reader.GetString(2);
                    string registre = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    string gsm = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    string telephone = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    string email = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    string rue = reader.IsDBNull(7) ? "" : reader.GetString(7);
                    string cp = reader.IsDBNull(8) ? "" : reader.GetString(8);
                    string localite = reader.IsDBNull(9) ? "" : reader.GetString(9);

                    Adresse adresse = new Adresse(rue, cp, localite);
                    Contact contact = new Contact(id, nom, prenom, registre, gsm, telephone, email, adresse);
                    contacts.Add(contact);
                }
                reader.Close();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la lecture des contacts ", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return contacts;
        }
        //add contact
        public int AjouterContact(Contact contact)
        {
            int contactId = 0;

            try
            {
                sqlConn.Open();
                string query = "INSERT INTO CONTACT (nom, prenom, registre_national, gsm, telephone, email, adresse_contact) " +
                               "VALUES (@nom, @prenom, @registre, @gsm, @telephone, @email, ROW(@rue, @cp, @localite)) " +
                               "RETURNING contact_identifiant";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@nom", contact.Nom);
                cmd.Parameters.AddWithValue("@prenom", contact.Prenom);
                cmd.Parameters.AddWithValue("@registre", contact.RegistreNational ?? "");
                cmd.Parameters.AddWithValue("@gsm", contact.GSM ?? "");
                cmd.Parameters.AddWithValue("@telephone", contact.Telephone ?? "");
                cmd.Parameters.AddWithValue("@email", contact.Email ?? "");
                cmd.Parameters.AddWithValue("@rue", contact.Adresse.Rue);
                cmd.Parameters.AddWithValue("@cp", contact.Adresse.Cp);
                cmd.Parameters.AddWithValue("@localite", contact.Adresse.Localite);

                // récup l'ID auto-généré
                contactId = Convert.ToInt32(cmd.ExecuteScalar());

                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de l'ajout d'un contact", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return contactId;
        }
        //teste si l'animal existe???
        public int ExisteAnimal(string identifiant)
        {
            int resultat = 0;
            try
            {
                sqlConn.Open();
                string query = "SELECT COUNT(*) FROM ANIMAL WHERE identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", identifiant);

                resultat = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                string detailsComplets = "Type: " + ex.GetType().Name + "\n";
                detailsComplets += "Message: " + ex.Message;
                if (ex.InnerException != null)
                {
                    detailsComplets += "\nInner: " + ex.InnerException.Message;
                }
                throw new ExceptionAccesBD("Erreur lors de la vérification existence animal", detailsComplets);
            }
            return resultat;
        }

        // Teste si le contact est la
        public int ExisteContact(int contactId)
        {
            int resultat = 0;
            try
            {  
                sqlConn.Open();
                string query = "SELECT COUNT(*) FROM CONTACT WHERE contact_identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", contactId);

                resultat = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Dispose();
                sqlConn.Close();

            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la vérification le contact existe", ex.Message);
            }
            return resultat;
        }

        // Ajout vaccination
        public int AjouterVaccination(Vaccination vac)
        {
            int ligneAffectees = 0;

            try
            {
                sqlConn.Open();

                string query = "INSERT INTO VACCINATION (vac_animal, vac_vaccin, vaccination_date) " +
                               "VALUES (@animal, @vaccin, @date)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@animal", vac.AnimalId);
                cmd.Parameters.AddWithValue("@vaccin", vac.Vaccin);
                cmd.Parameters.AddWithValue("@date", vac.Date);

                ligneAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de l'ajout d'une vaccination", ex.Message);
            }

            return ligneAffectees;
        }
        //Liste vaccination de l animal
        public List<Vaccination> ListeVaccinationsAnimal(string animalId)
        {
            List<Vaccination> vaccinations = new List<Vaccination>();
            try
            {
                sqlConn.Open();
                string query = "SELECT vac_animal, vac_vaccin, vaccination_date FROM VACCINATION " +
                               "WHERE vac_animal = @animal ORDER BY vaccination_date DESC";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@animal", animalId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    string animal = reader.GetString(0);
                    string vaccin = reader.GetString(1);
                    DateTime date = reader.GetDateTime(2);

                    Vaccination vac = new Vaccination(animal, vaccin, date);
                    vaccinations.Add(vac);
                }
                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de la lecture des vaccinations", ex.Message);
            }
            return vaccinations;
        }
        // enregistre une entrée d un animal
        public int AjouterEntreeAnimal(AnimalEntree entree)
        {
            int lignesAffectees = 0;
            try
            {
                // VALIDATION 1 : Vérifier que l'animal est vivant
                if (!AnimalEstVivant(entree.AnimalId))
                {
                    throw new ExceptionAccesBD("Impossible d'enregistrer une entrée", "L'animal est décédé");
                }

                // VALIDATION 2 : Vérifier que l'animal n'est pas déjà présent
                if (AnimalEstPresent(entree.AnimalId))
                {
                    throw new ExceptionAccesBD("Impossible d'enregistrer une entrée", "L'animal est déjà présent au refuge");
                }

                // VALIDATION 3 : Si raison='retour_adoption' → vérifier sortie précédente='adoption'
                if (entree.Raison == "retour_adoption")
                {
                    string derniereSortie = RaisonDerniereSortie(entree.AnimalId);
                    if (derniereSortie != "adoption")
                    {
                        throw new ExceptionAccesBD("Impossible d'enregistrer une entrée 'retour_adoption'",
                            "La dernière sortie n'était pas une adoption (raison: " + (derniereSortie ?? "aucune") + ")");
                    }
                }

                sqlConn.Open();
                string query = "INSERT INTO ANI_ENTREE (ani_identifiant, date_entree, raison, entree_contact) " +
                               "VALUES (@animal, @date, @raison, @contact)";
                NpgsqlCommand cmd = new NpgsqlCommand( query, sqlConn);

                // on colle l'heure actuelle sur la date choisie pour avoir un vrai timestamp
                DateTime dateAvecHeure = entree.DateEntree.Date + DateTime.Now.TimeOfDay;
                cmd.Parameters.AddWithValue("@animal", entree.AnimalId);
                cmd.Parameters.AddWithValue("@date", dateAvecHeure);
                cmd.Parameters.AddWithValue("@raison", entree.Raison);
                cmd.Parameters.AddWithValue("@contact", entree.ContactId);

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch(Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de l'enregistrement de l'entrée", ex.Message);
            }
            return lignesAffectees;
        }
        //enregistre la sortie d un animal
        public int AjouterSortieAnimal(AnimalSortie sortie)
        {
            int lignesAffectes = 0;
            try
            {
                // VALIDATION 1 : Si raison ≠ 'deces_animal' → vérifier que l'animal est vivant
                if (sortie.Raison != "deces_animal")
                {
                    if (!AnimalEstVivant(sortie.AnimalId))
                    {
                        throw new ExceptionAccesBD("Impossible d'enregistrer une sortie", "L'animal est déjà décédé");
                    }
                }

                // VALIDATION 2 : Vérifier que l'animal est pas deja sorti
                // AnimalEstPresent retourne false pour les animaux sans entree/sortie,
                // mais si l'animal existe et est vivant c'est ok pour une sortie
                if (!AnimalEstPresent(sortie.AnimalId) && AnimalADesSorties(sortie.AnimalId))
                {
                    throw new ExceptionAccesBD("Impossible d'enregistrer une sortie", "L'animal n'est pas présent au refuge");
                }

                // VALIDATION 3 : Si raison='adoption' → vérifier qu'une adoption acceptée existe
                if (sortie.Raison == "adoption")
                {
                    if (!AdoptionAccepteeExiste(sortie.AnimalId))
                    {
                        throw new ExceptionAccesBD("Impossible d'enregistrer une sortie 'adoption'",
                            "Aucune adoption acceptée n'existe pour cet animal");
                    }
                }

                sqlConn.Open();
                string query = "INSERT INTO ANI_SORTIE (ani_identifiant, date_sortie, raison, sortie_contact) " +
                               "VALUES (@animal, @date, @raison, @contact)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                // on colle l'heure actuelle sur la date choisie pour avoir un vrai timestamp
                DateTime dateAvecHeure = sortie.DateSortie.Date + DateTime.Now.TimeOfDay;
                cmd.Parameters.AddWithValue("@animal", sortie.AnimalId);
                cmd.Parameters.AddWithValue("@date", dateAvecHeure);
                cmd.Parameters.AddWithValue("@raison", sortie.Raison);
                cmd.Parameters.AddWithValue("@contact", sortie.ContactId);

                lignesAffectes = cmd.ExecuteNonQuery();

                // VALIDATION 4 : Si raison='deces_animal' → mettre à jour date_deces dans ANIMAL
                if (sortie.Raison == "deces_animal")
                {
                    string updateQuery = "UPDATE ANIMAL SET date_deces = @date WHERE identifiant = @id";
                    NpgsqlCommand updateCmd = new NpgsqlCommand(updateQuery, sqlConn);
                    updateCmd.Parameters.AddWithValue("@date", sortie.DateSortie);
                    updateCmd.Parameters.AddWithValue("@id", sortie.AnimalId);
                    updateCmd.ExecuteNonQuery();
                    updateCmd.Dispose();
                }

                cmd.Dispose();
                sqlConn.Close();

            }
            catch(ExceptionAccesBD)
            {
                // on laisse passer les erreurs de validation sans les re-emballer
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw;
            }
            catch(Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de l'enregistrement de la sortie", ex.Message);
            }
            return lignesAffectes;
        }
        //ajout l'adoption
        public int AjouterAdoption(Adoption adoption)
        {
            int ligneAffectes = 0;
            try
            {
                sqlConn.Open();
                string query= "INSERT INTO ADOPTION (ani_identifiant, date_demande, statut, adop_contact) " + 
                              "VALUES (@animal, @date, @statut, @contact)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@animal", adoption.AnimalId);
                cmd.Parameters.AddWithValue("@date", adoption.DateDemande);
                cmd.Parameters.AddWithValue("@statut", adoption.Statut);
                cmd.Parameters.AddWithValue("@contact", adoption.ContactId);

                ligneAffectes = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur lors de l'ajou de l'adoption", ex.Message);
            }
            return ligneAffectes;
        }

        // Supprime un animal (et toutes ses données liées en cascade)
        public int SupprimerAnimal(string animalId)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();

                // Supprimer dans l'ordre pour respecter les contraintes
                // 1. Vaccinations
                string deleteVac = "DELETE FROM VACCINATION WHERE vac_animal = @id";
                NpgsqlCommand cmdVac = new NpgsqlCommand(deleteVac, sqlConn);
                cmdVac.Parameters.AddWithValue("@id", animalId);
                cmdVac.ExecuteNonQuery();
                cmdVac.Dispose();

                // 2. Couleurs
                string deleteCoul = "DELETE FROM ANIMAL_COULEUR WHERE ani_identifiant = @id";
                NpgsqlCommand cmdCoul = new NpgsqlCommand(deleteCoul, sqlConn);
                cmdCoul.Parameters.AddWithValue("@id", animalId);
                cmdCoul.ExecuteNonQuery();
                cmdCoul.Dispose();

                // 3. Compatibilités
                string deleteComp = "DELETE FROM ANI_COMPATIBILITE WHERE ani_identifiant = @id";
                NpgsqlCommand cmdComp = new NpgsqlCommand(deleteComp, sqlConn);
                cmdComp.Parameters.AddWithValue("@id", animalId);
                cmdComp.ExecuteNonQuery();
                cmdComp.Dispose();

                // 4. Adoptions
                string deleteAdop = "DELETE FROM ADOPTION WHERE ani_identifiant = @id";
                NpgsqlCommand cmdAdop = new NpgsqlCommand(deleteAdop, sqlConn);
                cmdAdop.Parameters.AddWithValue("@id", animalId);
                cmdAdop.ExecuteNonQuery();
                cmdAdop.Dispose();

                // 5. Familles d'accueil
                string deleteFA = "DELETE FROM FAMILLE_ACCUEIL WHERE fa_ani_identifiant = @id";
                NpgsqlCommand cmdFA = new NpgsqlCommand(deleteFA, sqlConn);
                cmdFA.Parameters.AddWithValue("@id", animalId);
                cmdFA.ExecuteNonQuery();
                cmdFA.Dispose();

                // 6. Entrées
                string deleteEntree = "DELETE FROM ANI_ENTREE WHERE ani_identifiant = @id";
                NpgsqlCommand cmdEntree = new NpgsqlCommand(deleteEntree, sqlConn);
                cmdEntree.Parameters.AddWithValue("@id", animalId);
                cmdEntree.ExecuteNonQuery();
                cmdEntree.Dispose();

                // 7. Sorties
                string deleteSortie = "DELETE FROM ANI_SORTIE WHERE ani_identifiant = @id";
                NpgsqlCommand cmdSortie = new NpgsqlCommand(deleteSortie, sqlConn);
                cmdSortie.Parameters.AddWithValue("@id", animalId);
                cmdSortie.ExecuteNonQuery();
                cmdSortie.Dispose();

                // 8. Enfin l'animal lui-même
                string deleteAnimal = "DELETE FROM ANIMAL WHERE identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(deleteAnimal, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);
                lignesAffectees = cmd.ExecuteNonQuery();
                cmd.Dispose();

                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la suppression de l'animal", ex.Message);
            }
            return lignesAffectees;
        }

        // ============================================================
        // MÉTHODES FAMILLE D'ACCUEIL
        // ============================================================

        // Ajoute un placement en famille d'accueil
        public int AjouterFamilleAccueil(FamilleAccueil fa)
        {
            int lignesAffectees = 0;
            try
            {
                // VALIDATION : Vérifier qu'il n'y a pas déjà un placement actif pour cet animal
                sqlConn.Open();
                string checkQuery = "SELECT COUNT(*) FROM FAMILLE_ACCUEIL WHERE fa_ani_identifiant = @id AND date_fin IS NULL";
                NpgsqlCommand checkCmd = new NpgsqlCommand(checkQuery, sqlConn);
                checkCmd.Parameters.AddWithValue("@id", fa.AnimalId);

                int placementsActifs = Convert.ToInt32(checkCmd.ExecuteScalar());
                checkCmd.Dispose();

                if (placementsActifs > 0)
                {
                    sqlConn.Close();
                    throw new ExceptionAccesBD("Impossible d'ajouter une famille d'accueil",
                        "L'animal est déjà en famille d'accueil active");
                }

                string query = "INSERT INTO FAMILLE_ACCUEIL (fa_ani_identifiant, fa_contact, date_debut, date_fin) " +
                               "VALUES (@animal, @contact, @debut, @fin)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@animal", fa.AnimalId);
                cmd.Parameters.AddWithValue("@contact", fa.ContactId);
                cmd.Parameters.AddWithValue("@debut", fa.DateDebut);

                // date_fin peut être NULL
                if (fa.DateFin == DateTime.MinValue)
                {
                    cmd.Parameters.AddWithValue("@fin", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@fin", fa.DateFin);
                }

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de l'ajout famille d'accueil", ex.Message);
            }
            return lignesAffectees;
        }

        // Termine un placement en famille d'accueil (met à jour date_fin)
        public int TerminerFamilleAccueil(string animalId, DateTime dateFin)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = @"UPDATE FAMILLE_ACCUEIL
                                SET date_fin = @fin
                                WHERE fa_ani_identifiant = @id
                                AND date_fin IS NULL";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);
                cmd.Parameters.AddWithValue("@fin", dateFin);

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la fin famille d'accueil", ex.Message);
            }
            return lignesAffectees;
        }

        // Liste toutes les familles d'accueil d'un animal
        public List<FamilleAccueil> ListeFamillesAccueilAnimal(string animalId)
        {
            List<FamilleAccueil> familles = new List<FamilleAccueil>();
            try
            {
                sqlConn.Open();
                string query = @"SELECT fa_ani_identifiant, fa_contact, date_debut, date_fin
                                FROM FAMILLE_ACCUEIL
                                WHERE fa_ani_identifiant = @id
                                ORDER BY date_debut DESC";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string animal = reader.GetString(0);
                    int contact = reader.GetInt32(1);
                    DateTime debut = reader.GetDateTime(2);

                    FamilleAccueil fa = new FamilleAccueil(animal, contact, debut);

                    if (!reader.IsDBNull(3))
                    {
                        fa.DateFin = reader.GetDateTime(3);
                    }

                    familles.Add(fa);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture familles d'accueil", ex.Message);
            }
            return familles;
        }

        // Liste toutes les familles d'accueil
        public List<FamilleAccueil> ListeToutesFamillesAccueil()
        {
            List<FamilleAccueil> familles = new List<FamilleAccueil>();
            try
            {
                sqlConn.Open();
                string query = @"SELECT fa_ani_identifiant, fa_contact, date_debut, date_fin
                                FROM FAMILLE_ACCUEIL
                                ORDER BY date_debut DESC";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string animal = reader.GetString(0);
                    int contact = reader.GetInt32(1);
                    DateTime debut = reader.GetDateTime(2);

                    FamilleAccueil fa = new FamilleAccueil(animal, contact, debut);

                    if (!reader.IsDBNull(3))
                    {
                        fa.DateFin = reader.GetDateTime(3);
                    }

                    familles.Add(fa);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture de toutes les familles d'accueil", ex.Message);
            }
            return familles;
        }

        // Liste tous les animaux accueillis par une famille (contact) spécifique
        public List<Animal> ListeAnimauxParFamille(int contactId)
        {
            List<Animal> animaux = new List<Animal>();
            try
            {
                sqlConn.Open();
                string query = @"SELECT DISTINCT a.identifiant, a.nom, a.type, a.sexe, a.date_naissance
                                FROM ANIMAL a
                                INNER JOIN FAMILLE_ACCUEIL fa ON a.identifiant = fa.fa_ani_identifiant
                                WHERE fa.fa_contact = @contactId
                                ORDER BY a.nom";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@contactId", contactId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string identifiant = reader.GetString(0);
                    string nom = reader.GetString(1);
                    string type = reader.GetString(2);
                    char sexe = reader.GetChar(3);
                    DateTime dateNaissance = reader.GetDateTime(4);

                    Animal animal = new Animal(identifiant, nom, type, sexe, dateNaissance);
                    animaux.Add(animal);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture des animaux par famille", ex.Message);
            }
            return animaux;
        }

        // ============================================================
        // MÉTHODES ROLE CONTACT
        // ============================================================

        // Ajoute un rôle à un contact
        public int AjouterRoleContact(int contactId, int roleId)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = "INSERT INTO PERSONNE_ROLE (pers_identifiant, rol_identifiant) VALUES (@contact, @role)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@contact", contactId);
                cmd.Parameters.AddWithValue("@role", roleId);

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de l'ajout du rôle au contact", ex.Message);
            }
            return lignesAffectees;
        }

        // Liste tous les rôles d'un contact
        public List<Role> ListeRolesContact(int contactId)
        {
            List<Role> roles = new List<Role>();
            try
            {
                sqlConn.Open();
                string query = @"SELECT r.rol_identifiant, r.rol_nom
                                FROM ROLE r
                                INNER JOIN PERSONNE_ROLE pr ON r.rol_identifiant = pr.rol_identifiant
                                WHERE pr.pers_identifiant = @id";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", contactId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string nom = reader.GetString(1);

                    Role role = new Role(id, nom);
                    roles.Add(role);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture des rôles du contact", ex.Message);
            }
            return roles;
        }

        // ============================================================
        // MÉTHODES COULEUR ANIMAL
        // ============================================================

        // Ajoute une couleur à un animal
        public int AjouterCouleurAnimal(string animalId, string couleur)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = "INSERT INTO ANIMAL_COULEUR (ani_identifiant, nom_couleur) VALUES (@animal, @couleur)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@animal", animalId);
                cmd.Parameters.AddWithValue("@couleur", couleur);

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de l'ajout de la couleur", ex.Message);
            }
            return lignesAffectees;
        }

        // Liste toutes les couleurs d'un animal
        public List<string> ListeCouleursAnimal(string animalId)
        {
            List<string> couleurs = new List<string>();
            try
            {
                sqlConn.Open();
                string query = "SELECT nom_couleur FROM ANIMAL_COULEUR WHERE ani_identifiant = @id";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string couleur = reader.GetString(0);
                    couleurs.Add(couleur);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture des couleurs", ex.Message);
            }
            return couleurs;
        }

        // Supprime une couleur d'un animal
        public int SupprimerCouleurAnimal(string animalId, string couleur)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = "DELETE FROM ANIMAL_COULEUR WHERE ani_identifiant = @animal AND nom_couleur = @couleur";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@animal", animalId);
                cmd.Parameters.AddWithValue("@couleur", couleur);

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la suppression de la couleur", ex.Message);
            }
            return lignesAffectees;
        }

        // ============================================================
        // MÉTHODES COMPATIBILITÉ ANIMAL
        // ============================================================

        // Ajoute une compatibilité pour un animal
        public int AjouterAnimalCompatibilite(AnimalCompatibilite ac)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = @"INSERT INTO ANI_COMPATIBILITE (ani_identifiant, comp_type, valeur, description)
                                VALUES (@animal, @type, @valeur, @desc)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@animal", ac.AnimalId);
                cmd.Parameters.AddWithValue("@type", ac.CompType);
                cmd.Parameters.AddWithValue("@valeur", ac.Valeur);
                cmd.Parameters.AddWithValue("@desc", ac.Description ?? "");

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de l'ajout de la compatibilité", ex.Message);
            }
            return lignesAffectees;
        }

        // Liste toutes les compatibilités d'un animal (avec nom du type via JOIN)
        public List<AnimalCompatibilite> ListeCompatibilitesAnimal(string animalId)
        {
            List<AnimalCompatibilite> compatibilites = new List<AnimalCompatibilite>();
            try
            {
                sqlConn.Open();
                // JOIN avec COMPATIBILITE pour récup le nom du type
                string query = @"SELECT ac.ani_identifiant, ac.comp_type, c.type, ac.valeur, ac.description
                                FROM ANI_COMPATIBILITE ac
                                INNER JOIN COMPATIBILITE c ON ac.comp_type = c.comp_identifiant
                                WHERE ac.ani_identifiant = @id
                                ORDER BY ac.comp_type";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string animal = reader.GetString(0);
                    int compType = reader.GetInt32(1);
                    string typeNom = reader.GetString(2); // nom du type depuis COMPATIBILITE.type
                    bool valeur = reader.GetBoolean(3);
                    string description = reader.IsDBNull(4) ? "" : reader.GetString(4);

                    // utilise le constructeur avec 5 paramètres incluant typeNom
                    AnimalCompatibilite ac = new AnimalCompatibilite(animal, compType, typeNom, valeur, description);
                    compatibilites.Add(ac);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture des compatibilités", ex.Message);
            }
            return compatibilites;
        }

        // Supprime une compatibilité d'un animal
        public int SupprimerCompatibiliteAnimal(string animalId, int compType)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = "DELETE FROM ANI_COMPATIBILITE WHERE ani_identifiant = @animal AND comp_type = @type";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);

                cmd.Parameters.AddWithValue("@animal", animalId);
                cmd.Parameters.AddWithValue("@type", compType);

                lignesAffectees = cmd.ExecuteNonQuery();

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la suppression de la compatibilité", ex.Message);
            }
            return lignesAffectees;
        }

        // ============================================================
        // MÉTHODES CONSULTATION AVANCÉES
        // ============================================================

        // Liste toutes les entrées et sorties d'un animal (historique complet)
        public List<string> ListeEntreesSortiesAnimal(string animalId)
        {
            List<string> historique = new List<string>();
            try
            {
                sqlConn.Open();

                // Récupérer toutes les entrées et sorties, triées par date
                string query = @"
                    SELECT 'ENTREE' AS type, date_entree AS date, raison, entree_contact AS contact
                    FROM ANI_ENTREE
                    WHERE ani_identifiant = @id
                    UNION ALL
                    SELECT 'SORTIE' AS type, date_sortie AS date, raison, sortie_contact AS contact
                    FROM ANI_SORTIE
                    WHERE ani_identifiant = @id
                    ORDER BY date DESC";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string type = reader.GetString(0);
                    DateTime date = reader.GetDateTime(1);
                    string raison = reader.GetString(2);
                    int contact = reader.GetInt32(3);

                    string ligne = string.Format("{0} - {1} le {2} (raison: {3}, contact: {4})",
                        type, animalId, date.ToShortDateString(), raison, contact);
                    historique.Add(ligne);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture de l'historique", ex.Message);
            }
            return historique;
        }

        // Liste tout l'historique des entrées/sorties (tous animaux)
        public List<string> ListeToutHistorique()
        {
            List<string> historique = new List<string>();
            try
            {
                sqlConn.Open();

                // récupère toutes les entrées et sorties avec infos animal et contact
                string query = @"
                    SELECT 'ENTREE' AS type, e.date_entree AS date, e.ani_identifiant, a.nom AS animal_nom,
                           e.raison, c.prenom || ' ' || c.nom AS contact_nom
                    FROM ANI_ENTREE e
                    JOIN ANIMAL a ON e.ani_identifiant = a.identifiant
                    JOIN CONTACT c ON e.entree_contact = c.contact_identifiant
                    UNION ALL
                    SELECT 'SORTIE' AS type, s.date_sortie AS date, s.ani_identifiant, a.nom AS animal_nom,
                           s.raison, c.prenom || ' ' || c.nom AS contact_nom
                    FROM ANI_SORTIE s
                    JOIN ANIMAL a ON s.ani_identifiant = a.identifiant
                    JOIN CONTACT c ON s.sortie_contact = c.contact_identifiant
                    ORDER BY date DESC";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string type = reader.GetString(0);
                    DateTime date = reader.GetDateTime(1);
                    string animalId = reader.GetString(2);
                    string animalNom = reader.GetString(3);
                    string raison = reader.GetString(4);
                    string contactNom = reader.GetString(5);

                    historique.Add($"{type}|{date.ToShortDateString()}|{animalId}|{animalNom}|{raison}|{contactNom}");
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture de l'historique complet", ex.Message);
            }
            return historique;
        }

        // Liste toutes les adoptions (tous animaux confondus)
        public List<Adoption> ListeAdoptions()
        {
            List<Adoption> adoptions = new List<Adoption>();
            try
            {
                sqlConn.Open();
                string query = @"SELECT ani_identifiant, adop_contact, date_demande, statut
                                FROM ADOPTION
                                ORDER BY date_demande DESC";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string animalId = reader.GetString(0).Trim();
                    int contactId = reader.GetInt32(1);
                    DateTime dateDemande = reader.GetDateTime(2);
                    string statut = reader.GetString(3).Trim();

                    Adoption adoption = new Adoption(animalId, contactId, dateDemande);
                    adoption.Statut = statut;
                    adoptions.Add(adoption);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture des adoptions", ex.Message);
            }
            return adoptions;
        }

        // Liste les adoptions d'un animal spécifique
        public List<Adoption> ListeAdoptionsAnimal(string animalId)
        {
            List<Adoption> adoptions = new List<Adoption>();
            try
            {
                sqlConn.Open();
                string query = @"SELECT ani_identifiant, adop_contact, date_demande, statut
                                FROM ADOPTION
                                WHERE ani_identifiant = @id
                                ORDER BY date_demande DESC";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);
                NpgsqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string animal = reader.GetString(0);
                    int contactId = reader.GetInt32(1);
                    DateTime dateDemande = reader.GetDateTime(2);
                    string statut = reader.GetString(3);

                    Adoption adoption = new Adoption(animal, contactId, dateDemande);
                    adoption.Statut = statut;
                    adoptions.Add(adoption);
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la lecture des adoptions de l'animal", ex.Message);
            }
            return adoptions;
        }

        // Met à jour le statut d'une adoption
        public int UpdateStatutAdoption(string animalId, DateTime dateDemande, string nouveauStatut)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = @"UPDATE ADOPTION
                                SET statut = @statut
                                WHERE ani_identifiant = @animal AND date_demande = @date";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@animal", animalId);
                cmd.Parameters.AddWithValue("@date", dateDemande);
                cmd.Parameters.AddWithValue("@statut", nouveauStatut);

                lignesAffectees = cmd.ExecuteNonQuery();

                if (lignesAffectees == 0)
                {
                    cmd.Dispose();
                    sqlConn.Close();
                    throw new ExceptionAccesBD("Erreur mise à jour adoption",
                        "Aucune adoption trouvée pour cette combinaison animal/date");
                }

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la mise à jour du statut adoption", ex.Message);
            }
            return lignesAffectees;
        }

        // ============================================================
        // MÉTHODES DE VÉRIFICATION PRIVÉES POUR VALIDATIONS MÉTIER
        // ============================================================

        // Vérifie si un animal est vivant (date_deces = NULL)
        private bool AnimalEstVivant(string animalId)
        {
            bool estVivant = true;
            try
            {
                sqlConn.Open();
                string query = "SELECT date_deces FROM ANIMAL WHERE identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    estVivant = false; // Animal décédé
                }

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la vérification si animal vivant", ex.Message);
            }
            return estVivant;
        }

        // Vérifie si l'animal est présent au refuge (dernière action = entrée)
        private bool AnimalEstPresent(string animalId)
        {
            bool estPresent = false;
            try
            {
                sqlConn.Open();
                // on compare le nombre d'entrees vs sorties, plus fiable que les dates
                string query = @"
                    SELECT
                        (SELECT COUNT(*) FROM ANI_ENTREE WHERE ani_identifiant = @id) AS nb_entrees,
                        (SELECT COUNT(*) FROM ANI_SORTIE WHERE ani_identifiant = @id) AS nb_sorties";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                NpgsqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    int nbEntrees = reader.GetInt32(0);
                    int nbSorties = reader.GetInt32(1);
                    // present si plus d'entrees que de sorties
                    estPresent = nbEntrees > nbSorties;
                }

                reader.Close();
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la vérification de présence animal", ex.Message);
            }
            return estPresent;
        }

        // check si l'animal a au moins une sortie dans ANI_SORTIE
        private bool AnimalADesSorties(string animalId)
        {
            bool aDesSorties = false;
            try
            {
                sqlConn.Open();
                string query = "SELECT COUNT(*) FROM ANI_SORTIE WHERE ani_identifiant = @id";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                aDesSorties = count > 0;
                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur verif sorties animal", ex.Message);
            }
            return aDesSorties;
        }

        // Récupère la raison de la dernière sortie
        private string RaisonDerniereSortie(string animalId)
        {
            string raison = null;
            try
            {
                sqlConn.Open();
                string query = @"SELECT raison FROM ANI_SORTIE
                                WHERE ani_identifiant = @id
                                ORDER BY date_sortie DESC LIMIT 1";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    raison = result.ToString();
                }

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la récupération dernière sortie", ex.Message);
            }
            return raison;
        }

        // supprime une adoption (par animal + date_demande = cle primaire)
        public int SupprimerAdoption(string animalId, DateTime dateDemande)
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = "DELETE FROM ADOPTION WHERE ani_identifiant = @animal AND date_demande = @date";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@animal", animalId);
                cmd.Parameters.AddWithValue("@date", dateDemande);

                lignesAffectees = cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur suppression adoption", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return lignesAffectees;
        }

        // vire les adoptions orphelines (animal ou contact supprime de la BD)
        public int NettoyerAdoptionsOrphelines()
        {
            int lignesAffectees = 0;
            try
            {
                sqlConn.Open();
                string query = @"DELETE FROM ADOPTION
                                WHERE ani_identifiant NOT IN (SELECT identifiant FROM ANIMAL)
                                OR adop_contact NOT IN (SELECT contact_identifiant FROM CONTACT)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                lignesAffectees = cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                throw new ExceptionAccesBD("Erreur nettoyage adoptions orphelines", ex.Message);
            }
            finally
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
            }
            return lignesAffectees;
        }

        // Vérifie si une adoption acceptée existe pour l'animal
        private bool AdoptionAccepteeExiste(string animalId)
        {
            bool existe = false;
            try
            {
                sqlConn.Open();
                string query = @"SELECT COUNT(*) FROM ADOPTION
                                WHERE ani_identifiant = @id AND LOWER(TRIM(statut)) = 'acceptee'";

                NpgsqlCommand cmd = new NpgsqlCommand(query, sqlConn);
                cmd.Parameters.AddWithValue("@id", animalId);

                int count = Convert.ToInt32(cmd.ExecuteScalar());
                existe = count > 0;

                cmd.Dispose();
                sqlConn.Close();
            }
            catch (Exception ex)
            {
                if (sqlConn.State == System.Data.ConnectionState.Open)
                    sqlConn.Close();
                throw new ExceptionAccesBD("Erreur lors de la vérification adoption acceptée", ex.Message);
            }
            return existe;
        }
    }
}
