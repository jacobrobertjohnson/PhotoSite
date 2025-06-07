#!/bin/bash

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
CONTAINER_DIR="$(dirname "$SCRIPT_DIR")"
ROOT_DIR="$(dirname "$CONTAINER_DIR")"
DATA_DIR="$CONTAINER_DIR/data"
APPS_DIR="$CONTAINER_DIR/apps"
SRC_DIR="$ROOT_DIR/src"
IMPORTER_DIR="$DATA_DIR/importer"

ERROR_LOG_PATH="$DATA_DIR/errors"
PHOTO_FILE_PATH="$DATA_DIR/library/photos"
PHOTO_THUMBNAIL_PATH="$DATA_DIR/library/thumbnails"
IMPORTER_CONFIG_PATH="$DATA_DIR/importerConfig.json"
SITE_CONFIG_PATH="$SRC_DIR/appsettings.Development.json"

PHOTO_DB_PATH="$DATA_DIR/Photos.db"
USER_DB_PATH="$DATA_DIR/Users.db"

USER_SQL_PATH="$SCRIPT_DIR/adduser.sql"

# Seed PhotoImporter configs and sample data
cd "$CONTAINER_DIR"
mkdir apps
mkdir data
cd data
mkdir importer

cp "$SCRIPT_DIR/images/"* "$IMPORTER_DIR"

rm "$IMPORTER_CONFIG_PATH"
echo $(jq -n \
    --arg SourceDir "$IMPORTER_DIR" \
    --arg SourceFilePattern "jpg,jpeg,png,bmp" \
    --arg DatabasePath "$PHOTO_DB_PATH" \
    --arg StoragePath "$PHOTO_FILE_PATH" \
    --arg ThumbnailPath "$PHOTO_THUMBNAIL_PATH" \
    --arg VerboseOutput "true" \
    '{SourceDir: $SourceDir, SourceFilePattern: $SourceFilePattern, DatabasePath: $DatabasePath, StoragePath: $StoragePath, ThumbnailPath: $ThumbnailPath, VerboseOutput: $VerboseOutput }'
) > "$IMPORTER_CONFIG_PATH"

# Set up and run PhotoImporter
cd "$APPS_DIR"
git clone https://github.com/jacobrobertjohnson/PhotoImporter

cd PhotoImporter
rm -rf .git

cd src
dotnet build
PhotoImporter/bin/Debug/net9.0/PhotoImporter --configFile $IMPORTER_CONFIG_PATH

# Generate and install the dotnet dev certificate
dotnet dev-certs https

# Generate the PhotoSite config
cd "$SRC_DIR"

echo $(jq -n \
    --arg udb "$USER_DB_PATH" \
    --arg elp "$ERROR_LOG_PATH" \
    --arg mc "5fec3838-9d87-41ad-8c6f-54514abd43c5" \
    --arg fid "TestFamily" \
    --arg fnm "Test Family" \
    --arg pdb "$PHOTO_DB_PATH" \
    --arg pfp "$PHOTO_FILE_PATH" \
    --arg ptp "$PHOTO_THUMBNAIL_PATH" \
    '{ AppSettings: { UserDbPath: $udb, ErrorLogPath: $elp, MachineKey: $mc, Families: [{ Id: $fid, Name: $fnm, PhotoDbPath: $pdb, PhotoFilePath: $pfp, PhotoThumbnailpath: $ptp }] } }'
) > "$SITE_CONFIG_PATH"

sqlite3 "$USER_DB_PATH" < "$USER_SQL_PATH"