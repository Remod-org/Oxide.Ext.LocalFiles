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
  This Oxide plugin is built to work with the LocalFiles Extension.  You can add and remove files from multiple categories.
  
### Configuration
  There is no configuration for this plugin.

### Commands

  - /file
    - /file list
    - /file get URL -- Fetches a file at a specified URL.  You can also use /file url or /file fetch
    - /file info FILEKEY -- Displays info for a file based on the key from the list command above
    - /file delete FILEKEY -- Delete the file based on the key from the list command.  You can also use /file remove.
    
    - /file rename OLDNAME NEWNAME
    - /file category FILEKEY CATEGORYNAME -- Apply an arbitrary category name to a file.  There is no lookup for a fixed list of categories, so they can be created
    on the fly.  To remain consistent, simply use the same CATEGORYNAME at all times.  You may also use /file cat.
	- /file uncat FILEKEY CATEGORYNAME -- Remove file from category

### Permissions
  Currently, only admins can use this plugin


## TimeArtist.cs
  This plugin is designed to schedule sign painting using SignArtist to paint the signs while using the LocalFiles Extension to source those files.

### Configuration

```json
{
  "enabled": true,
  "debug": false,
  "rotPeriod": 30.0,
  "distance": 3.0,
  "UseLocalFiles": true,
  "Version": {
    "Major": 1,
    "Minor": 0,
    "Patch": 2
  }
}
```

  - `rotPeriod` -- How often to cycle signs in seconds.  This standard period can be skipped for X number of cycles per sign.
  - `distance`  -- How close do you need to be to the sign in game meters to manage it?
  - `UseLocalFiles` -- If true, manage files using the LocalFiles Extension

### Commands

  - /ta -- While looking at a sign, attempts to add the sign to its database for managing updates.
    If the sign has already been added, displays info.

    - /ta remove -- Removes the sign from the database
	- /ta urla -- Add a URL to the sign
	- /ta urlr -- Remove the specified URL from the sign (list)

	- /ta urlc -- Sets a category for use of a specific category in the LocalFiles Extension
	  Will clear all existing URLs for the sign, replacing them with the single category
	
	- /ta skip -- How many cycles to skip.  Used to delay rotation time for a sign.

	- /ta enable -- Toggles enabling image rotation

### Permissions
  Currently, only admins can use this plugin

