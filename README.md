#Configuration file structure
Tcf-backup uses yaml with instructions how and what to backup.
Every config should contains 3 nodes:
* source
* actions
* target

Where source and target are single nodes when actions is list of nodes.
Below are config format specification and examples:

##Source
```yaml
# Source: Directory
# Parameters:
#   type - directory
#   path - absolute path to source directory
source:
  type: directory
  path: /dev/null
---

# Source: Btrfs subvolume
# Parameters:
#   type - btrfs
#   subvolume - absolute path to subvolume which will be used as source
#   snapshotdir - optional absolute path. If specified program will create
#                 readonly snapshot in specified directory and use it as source
source:
  type: btrfs
  subvolume: /dev/null
  snapshotdir: /dev/null/snapshot
---

# Source: LXD
# Parameters:
#   type - lxd
#   containers - list of containers names for backup
#   ignoremissing - optional bool. Just skip containers listed above if they don't exists
#                   false by default
# Program will execute `lxc export --instance-only` for each container
# the feed result archives as source to actions
source:
  type: lxd
  containers:
    - container1
    - container2
    - container3
  ignoremissing: true
---

```
##Actions
```yaml
# Action: Filter
# Parameters:
#   include - list of regexes, will be applied to absolute path of each file
#   exclude - list of regexes, will be applied to absolute path of each file
#   followsymlinks - optional, default false
#                    controls will this action visit symlinked directory or not
# This action requires specified `include`, `exclude` or both
actions:
    - type: filter
      followsymlinks: true
      include:
        - .*/foo
        - .*/bar$
      exclude:
        - .*/foo\.exclude
        - .*/bar\.exclude
---

# Action: Compress
# Parameters:
#   algorithm - one of this values: bzip2, xz, lzip, lzma, lzop, zstd, gzip
#   followsymlinks - optional, dereference symlinks
#   changedir - optional, store files relative to specified directory
#   name - optional, archive name. If not specified random name will be generated
# Compress action uses `tar` to perform compression,
# so resulting archive will be of type .tar.[algorithm]
actions:
  - type: compress
    algorithm: gzip
    changedir: /mnt/btrfs/subvolume/snapshot
    name: snapshot
---

# Action: Encrypt
# Paramters:
#   engine - only one value supported: gpg
#   keyfile - path to exported gpg key
#   signature - signature of existing key
# This action performs encryption with gpg library
# You must specify only `keyfile` or `signature`, not both
actions:
  - type: encrypt
    engine: gpg
    signature: D24F6CB2C1B52632
  - type: encrypt
    engine: gpg
    keyfile: /root/gpg/key
---

# Action: Rename
# Parameters:
#   template - renaming template, can contains text and any count of this placeholders:
#              {filename} - full filename
#              {filename_without_ext} - same, but without extension
#              {ext} - only extension
#              {date} - current date
#              Date placeholder can have format, eg {date:HH:mm}
actions:
  - type: rename
    template: {filename_without_ext}-{date:dd-MM-yyyy-HH-mm}.{ext}
```
##Target
```yaml
# Target: directory
# Parameters:
#   path - absolute path to target directory
#   overwrite - optional, controls action behaviour if file with this name already exists
target:
  type: directory
  path: /mnt/backup/
  overwrite: true
---

# Target: gdrive
# Parameters:
#   path - path in google drive cloud. Directory tree created if not exists
target:
  type: gdrive
  path: backups
```


##Examples

###Example 1
```yaml
source:
  type: directory
  path: /mnt/data
actions:
  - type: filter
    exclude:
      - ^/mnt/data/*.\.tmp$

  - type: compress
    algorithm: gzip
    
  - type: rename
    template: data-{date:dd-MM-yyyy-HH-mm}.{ext}
target:
  type: directory
  path: /mnt/backup/
  overwrite: true
```

###Example 2
```yaml
source:
  type: directory
  path: /mnt/data
actions:
  - type: filter
    include:
      - ^/mnt/data/.*/.*?\.log$

  - type: compress
    algorithm: xz
    name: logs
    
  - type: encrypt
    engine: gpg
    signature: D24F6CB2C1B52632
    
  - type: rename
    template: {filename_without_ext}-{date:dd-MM-yyyy-HH-mm}.xz.gpg
target:
  type: gdrive
  path: backups
```

##TODO
 - [ ] Description of program
 - [ ] Binaries
 - [ ] Move documentation to wiki