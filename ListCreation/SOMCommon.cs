using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.SharePoint;
using System.Collections.Specialized;

namespace ListCreation
{
    public class SOMCommon
    {
        private SPSite site;

        public void setSPSite(SPSite context)
        {
            site = context;
        }

        #region Site level features

        public void createSubsite(string title, string url, string description, uint language, bool inheritParentPermission, string webTemplate)
        {
            try
            {
                SPWeb web = site.OpenWeb();

                var newWeb = web.Webs.Cast<SPWeb>().Where(w => w.Title.ToUpper() == title.ToUpper()).FirstOrDefault();

                if (newWeb == null)
                {
                    web.Webs.Add(url, title, description, language, webTemplate, inheritParentPermission, false);
                }
                else
                {
                    //return message : site already exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createSubsiteUsingTemplate(string title, string url, string description, uint language, bool inheritParentPermission, string templateName)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var newWeb = web.Webs.Cast<SPWeb>().Where(w => w.Title.ToUpper() == title.ToUpper()).FirstOrDefault();

                if (newWeb == null)
                {
                    SPWebTemplateCollection templates = web.GetAvailableWebTemplates(1033, true);

                    var template = templates.Cast<SPWebTemplate>().Where(t => t.Title == templateName).FirstOrDefault();

                    if (template != null)
                    {
                        web.Webs.Add(url, title, description, language, template.Name, inheritParentPermission, false);
                    }
                    else
                    {
                        //return message : template not exists.
                    }
                }
                else
                {
                    //return message : site already exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void deleteSubsite(string title)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                SPWeb targetSite = web.Webs.Cast<SPWeb>().Where(w => w.Title == title).FirstOrDefault();

                if (targetSite != null)
                {
                    web.Webs.Delete(title);
                }
                else
                {
                    //return message : site not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createGroup(string groupName, string description, SPRoleType groupRole)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                SPGroupCollection groupCollection = web.SiteGroups;

                var group = groupCollection.Cast<SPGroup>().Where(g => g.Name == groupName).FirstOrDefault();

                if (group == null)
                {
                    groupCollection.Add(groupName, web.Author, web.Author, description);

                    SPGroup newgroup = web.SiteGroups[groupName];
                    SPRoleDefinition roleDefinition = web.RoleDefinitions.GetByType(groupRole);
                    SPRoleAssignment roleAssignment = new SPRoleAssignment(newgroup);
                    roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                    web.RoleAssignments.Add(roleAssignment);
                    web.Update();
                }
                else
                {
                    //return message group already exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void addUserToGroup(string groupName, string loginName, string DisplayName, string email, string notes)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                SPGroupCollection groupCollection = web.SiteGroups;

                var group = groupCollection.Cast<SPGroup>().Where(g => g.Name == groupName).FirstOrDefault();

                if (group != null)
                {
                    SPUserCollection users = group.Users;
                    var user = users.Cast<SPUser>().Where(u => u.LoginName.ToUpper() == loginName.ToUpper()).FirstOrDefault();

                    if (user == null)
                    {
                        users.Add(loginName, email, DisplayName, notes);
                    }
                    else
                    {
                        //return message user already exists
                    }
                }
                else
                {
                    //return message group not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void removeUserFromGroup(string groupName, string loginName)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                SPGroupCollection groupCollection = web.SiteGroups;

                var group = groupCollection.Cast<SPGroup>().Where(g => g.Name == groupName).FirstOrDefault();

                if (group != null)
                {
                    SPUserCollection users = group.Users;
                    var user = users.Cast<SPUser>().Where(u => u.LoginName.ToUpper() == loginName.ToUpper()).FirstOrDefault();

                    if (user != null)
                    {
                        users.Remove(loginName);
                    }
                    else
                    {
                        //return message user not exists
                    }
                }
                else
                {
                    //return message group not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void deleteGroup(string groupName)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                SPGroupCollection groupCollection = web.SiteGroups;

                var group = groupCollection.Cast<SPGroup>().Where(g => g.Name.ToUpper() == groupName.ToUpper()).FirstOrDefault();

                if (group != null)
                {
                    groupCollection.Remove(groupName);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion

        #region List level features

        public void createList(string listName, string description, SPListTemplateType listTemplateType)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                SPListCollection lists = web.Lists;

                var list = lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list == null)
                {
                    lists.Add(listName, description, listTemplateType);
                }
                else
                {
                    //return message list already exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void deleteList(string listName)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                SPListCollection lists = web.Lists;

                var list = lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    lists.Delete(list.ID);
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createSingleLineTextField(string listName, string fieldName, string description, bool isRequired, string defaultValue)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.Text, isRequired);

                    SPFieldText fieldText = (SPFieldText)fieldCol[fieldName];
                    fieldText.Description = description;
                    fieldText.DefaultValue = defaultValue;
                    fieldText.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldText);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createMultiLineTextField(string listName, string fieldName, string description, bool isRequired, bool isRichText, string defaultValue)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.Note, isRequired);

                    SPFieldMultiLineText fieldMultiLine = (SPFieldMultiLineText)fieldCol[fieldName];
                    fieldMultiLine.Description = description;
                    fieldMultiLine.RichText = isRichText;
                    fieldMultiLine.DefaultValue = defaultValue;
                    fieldMultiLine.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldMultiLine);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createChoiceField(string listName, string fieldName, string description, bool isRequired, StringCollection choices, string defaultChoice, SPChoiceFormatType choiceType)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.Choice, isRequired, false, choices);

                    SPFieldChoice fieldChoice = (SPFieldChoice)fieldCol[fieldName];
                    fieldChoice.Description = description;
                    fieldChoice.EditFormat = choiceType;
                    if (!string.IsNullOrEmpty(defaultChoice))
                    {
                        fieldChoice.DefaultValue = defaultChoice;
                    }
                    fieldChoice.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldChoice);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createMultiChoiceField(string listName, string fieldName, string description, bool isRequired, StringCollection choices, string defaultChoice)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.MultiChoice, isRequired, false, choices);

                    SPFieldMultiChoice fieldMultiChoice = (SPFieldMultiChoice)fieldCol[fieldName];
                    fieldMultiChoice.Description = description;
                    if (!string.IsNullOrEmpty(defaultChoice))
                    {
                        fieldMultiChoice.DefaultValue = defaultChoice;
                    }
                    fieldMultiChoice.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldMultiChoice);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createNumberField(string listName, string fieldName, string description, bool isRequired, string minValue, string maxValue, string defaultValue)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.Number, isRequired);

                    SPFieldNumber fieldNumber = (SPFieldNumber)fieldCol[fieldName];
                    fieldNumber.Description = description;
                    fieldNumber.DefaultValue = defaultValue;
                    if (!string.IsNullOrEmpty(minValue))
                    {
                        fieldNumber.MinimumValue = Convert.ToDouble(minValue);
                    }
                    if (!string.IsNullOrEmpty(maxValue))
                    {
                        fieldNumber.MaximumValue = Convert.ToDouble(maxValue);
                    }
                    fieldNumber.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldNumber);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createDateTimeField(string listName, string fieldName, string description, bool isRequired, SPDateTimeFieldFormatType displayFormat, string defaultValue)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.DateTime, isRequired);

                    SPFieldDateTime fieldDateTime = (SPFieldDateTime)fieldCol[fieldName];
                    fieldDateTime.Description = description;
                    fieldDateTime.DisplayFormat = displayFormat;
                    fieldDateTime.DefaultValue = defaultValue;

                    fieldDateTime.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldDateTime);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createYesNoField(string listName, string fieldName, string description, bool defaultVaule)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.Boolean, false);

                    SPField oField = fieldCol[fieldName];
                    oField.Description = description;
                    oField.DefaultValue = defaultVaule == true ? "1" : "0";
                    oField.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(oField);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createUserField(string listName, string fieldName, string description, bool isRequired, SPFieldUserSelectionMode selectionMode, bool allowMultiple)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.User, false);

                    SPFieldUser userField = (SPFieldUser)fieldCol[fieldName];
                    userField.Description = description;
                    userField.SelectionMode = selectionMode;
                    userField.AllowMultipleValues = allowMultiple;
                    userField.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(userField);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createLookupField(string listName, string fieldName, string description, bool isRequired, Guid sourceListId, string sourceFieldName)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.AddLookup(fieldName,sourceListId,isRequired);

                    SPFieldLookup fieldLookup = (SPFieldLookup)fieldCol[fieldName];
                    fieldLookup.Required = isRequired;
                    fieldLookup.LookupField = sourceFieldName;
                    fieldLookup.Description = description;
                    fieldLookup.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldLookup);
                    defaultView.Update();
                }
                else
                {
                    //return message list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void createCalculatedField(string listName, string fieldName, string description, string formula, SPFieldType outputType)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    fieldCol.Add(fieldName, SPFieldType.Calculated, false);

                    SPFieldCalculated fieldCalculated = (SPFieldCalculated)fieldCol[fieldName];
                    fieldCalculated.Description = description;
                    fieldCalculated.Formula = formula;
                    fieldCalculated.OutputType = outputType;
                    fieldCalculated.Update();

                    SPView defaultView = list.DefaultView;
                    defaultView.ViewFields.Add(fieldCalculated);
                    defaultView.Update();
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void deleteField(string listName, string fieldName)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                var list = web.Lists.Cast<SPList>().Where(l => l.Title.ToUpper() == listName.ToUpper()).FirstOrDefault();

                if (list != null)
                {
                    SPFieldCollection fieldCol = list.Fields;
                    var field = fieldCol.Cast<SPField>().Where(f => f.Title.ToUpper() == fieldName.ToUpper()).FirstOrDefault();

                    if(field != null)
                    {
                        fieldCol.Delete(fieldName);
                    }
                    else
                    {
                        //return field not exists
                    }
                }
                else
                {
                    //return list not exists
                }
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        #endregion

        #region Item level features

        public SPListItemCollection getListItemCollection(string listName, string camlquery, SPWeb web)
        {
            try
            {
                SPListItemCollection items = null;
                SPList list = web.Lists.TryGetList(listName);

                if (list != null)
                {
                    SPQuery query = new SPQuery();
                    query.Query = camlquery;
                    items = list.GetItems(query); 
                }
                
                return items;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SPListItem getListItemById(string listName, int itemId, SPWeb web)
        {
            try
            {
                SPListItem item = null;
                SPList list = web.Lists.TryGetList(listName);

                if (list != null)
                {
                    item = list.GetItemById(itemId); 
                }
                return item;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int createListItem(string listName, object obj, SPWeb web)
        {
            int id = -1;
            try
            {
                Type type = obj.GetType();
                object objEntity = Activator.CreateInstance(type);
                objEntity = obj;
                PropertyInfo[] propertyInfo = objEntity.GetType().GetProperties();

                SPList list = web.Lists.TryGetList(listName);
                if (list != null)
                {
                    SPListItem item = list.Items.Add();

                    foreach (PropertyInfo pi in propertyInfo)
                    {
                        string fieldValue = Convert.ToString(pi.GetValue(objEntity, null));

                        if (fieldValue != DateTime.MinValue.ToString())
                        {
                            item[pi.Name] = pi.GetValue(objEntity, null);
                        }
                    }
                    web.AllowUnsafeUpdates = true;
                    item.Update();
                    id = item.ID;
                }

                return id;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        public void updateListItem(string listName, object obj, int itemId, SPWeb web)
        {
            try
            {
                Type type = obj.GetType();
                object objEntity = Activator.CreateInstance(type);
                objEntity = obj;
                PropertyInfo[] propertyInfo = objEntity.GetType().GetProperties();

                SPListItem item = getListItemById(listName, itemId, web);

                if (item != null)
                {
                    foreach (PropertyInfo pi in propertyInfo)
                    {
                        string fieldValue = Convert.ToString(pi.GetValue(objEntity, null));
                        Console.WriteLine(fieldValue);
                        if (fieldValue != DateTime.MinValue.ToString())
                        {
                            item[pi.Name] = pi.GetValue(objEntity, null);
                        }
                    }

                    web.AllowUnsafeUpdates = true;
                    item.Update(); 
                }
                else
                {
                    //do nothing
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        public void deleteListItem(string listName, int itemId, SPWeb web)
        {
            try
            {
                web.AllowUnsafeUpdates = true;
                SPListItem item = getListItemById(listName, itemId, web);

                if (item != null)
                {
                    item.Delete(); 
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        public void addAttachmentToListItem(string listName, int itemId, string fileName, byte[] file, SPWeb web)
        {
            try
            {
                web.AllowUnsafeUpdates = true;
                SPListItem item = getListItemById(listName, itemId, web);

                if (item != null)
                {
                    SPAttachmentCollection attachCol = item.Attachments;
                    attachCol.Add(fileName, file);
                    item.Update(); 
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        #endregion

        #region Library level features

        public int uploadFileInDocLibrary(string docLibraryName, byte[] fileInBytes, string fileNameWithExtension, bool overwrite, SPWeb web)
        {
            int itemID = -1;
            try
            {
                web.AllowUnsafeUpdates = true;
                SPList docLibrary = web.Lists[docLibraryName];
                SPFile file = docLibrary.RootFolder.Files.Add(fileNameWithExtension, fileInBytes, overwrite);
                itemID = file.Item.ID;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
            return itemID;
        }

        public void uploadFileInFolder(string folderRelativeUrl, byte[] fileInBytes, string fileNameWithExtension, SPWeb web)
        {
            try
            {
                web.AllowUnsafeUpdates = true;
                bool exists = web.GetFolder(folderRelativeUrl).Exists;//web.Folders.Cast<SPFolder>().Where(f => f.Url.ToUpper() == folderRelativeUrl.ToUpper()).FirstOrDefault();

                if (exists)
                {
                    SPFolder folder = web.GetFolder(folderRelativeUrl);
                    SPFile file = folder.Files.Add(fileNameWithExtension, fileInBytes);
                }
                else
                {
                    //return error folder not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        public void renameExistingFile(string docLibraryName, string fileLocationRelativePath, string oldfileName, string newFileName)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                bool exists = web.GetFolder(fileLocationRelativePath).Exists;
                
                if(exists)
                {
                    SPFolder folder = web.GetFolder(fileLocationRelativePath);
                    foreach(SPFile file in folder.Files)
                    {
                        if(file.Name.ToUpper() == oldfileName.ToUpper())
                        {
                            try
                            {
                                SPListItem item = file.Item;
                                item.File.CheckOut();
                                item.File.MoveTo(fileLocationRelativePath + "/" + newFileName, SPMoveOperations.Overwrite);
                                string comment = string.Empty;
                                item.File.CheckIn(comment, SPCheckinType.MinorCheckIn);
                                item.Update();
                            }
                            catch (Exception)
                            {
                            }
                            break;
                        }
                    }
                }
                else
                {
                    //return error folder not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void deleteFileFromDocLibrary(string fileRelativeUrl, SPWeb web)
        {
            try
            {
                web.AllowUnsafeUpdates = true;
                bool exists = web.GetFile(fileRelativeUrl).Exists;

                if (exists)
                {
                    SPFile file = web.GetFile(fileRelativeUrl);
                    file.Delete();
                }
                else
                {
                    //return file not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                web.AllowUnsafeUpdates = false;
            }
        }

        public void createFolderInDocLibrary(string docLibraryName, string destinationRelativeUrl, string folderName)
        {
            string folderUrl = destinationRelativeUrl + "/" + folderName;

            try
            {
                SPWeb web = site.OpenWeb();
                bool exists = web.GetFolder(folderUrl).Exists;

                if (!exists)
                {
                    SPList docLibrary = web.Lists[docLibraryName];
                    docLibrary.RootFolder.SubFolders.Add(folderUrl);
                }
                else
                {
                    //Return error folder already exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void deleteFolderOfDocLibrary(string folderRelativeUrl)
        {
            try
            {
                SPWeb web = site.OpenWeb();
                bool exists = web.GetFolder(folderRelativeUrl).Exists;

                if(exists)
                {
                    SPFolder folder = web.GetFolder(folderRelativeUrl);
                    folder.Delete();
                }
                else
                {
                    //return folder not exists
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public byte[] getFileFromDocLibrary(string docLibraryName, int itemId, SPWeb web, out string fileName)
        {
            byte[] byteFile = null;
            fileName = string.Empty;

            try
            {
                SPListItem listItem = getListItemById(docLibraryName, itemId, web);

                if (listItem != null)
                {
                    byteFile = listItem.File.OpenBinary();
                    fileName = listItem.File.Name; 
                }
                else
                {
                    //do nothing
                }
            }
            catch (Exception)
            {
                throw;
            }

            return byteFile;
        }

        #endregion
    }
}
