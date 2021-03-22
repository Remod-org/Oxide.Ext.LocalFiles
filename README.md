# Oxide.Ext.LocalFiles
Manage localfiles

Metadata for managed files are stored at OXIDEFOLDER/data/localfiles:
  - FileMeta.json
    - Contains file metadata including source URL, file size, H/W for images, and mimetype
  - FileToKey.json
    - Reverse lookup using filename to key in FileMeta.json

Actual files (content) are saved in OXIDEFOLDER/data/localfiles/Content.
In theory, you can have multiple subfolders here, and they should all be managed and processed.

## Caveats
  Do NOT manage/edit the above data files manually unless you would like to restart your Rust server to make use of the changes.
  Without a server restart, their contents will remain out of sync.
  On restart, new files will be populated into the above data files.

## FileManager.cs
  This Oxide plugin is built to work with the LocalFiles Extension.
  
### Commands

  - /file
    - /file list
    - /file get URL -- Fetches a file at a specified URL.  You can also use /file url or /file fetch
    - /file info FILEKEY -- Displays info for a file based on the key from the list command above
    - /file delete FILEKEY -- Delete the file based on the key from the list command.  You can also use /file remove.
    
    - /file rename OLDNAME NEWNAME
    - /file category FILEKEY CATEGORYNAME -- Apply an arbitrary category name to a file.  There is no lookup for a fixed list of categories, so they can be created
    on the fly.  To remain consistent, simply use the same CATEGORYNAME at all times.  You may also use /file cat.

### Permissions
  Currently, only admins can use this plugin
  