#!/bin/env python3

import os
import shutil
import subprocess as sp

from pathlib import Path

TCF_BACKUP = Path(os.path.dirname(os.path.realpath(__file__))) / '..' / 'publish' / 'publish' / 'linux-x64' / 'portable' / 'dist' / 'usr' / 'bin' / 'tcf-backup'
CONFIG_DIR = Path(os.path.dirname(os.path.realpath(__file__))) / 'config'

SIMPLE_FILES = ['exclude_file_1',
            'exclude_file_2',
            'exclude_dir_1/file1',
            'exclude_dir_1/file2',
            'exclude_dir_2/file1',
            'exclude_dir_2/file2',
            'file_1',
            'file_2',
            'dir_1/file1',
            'dir_1/file2',
            'dir_2/file1',
            'dir_2/file2']

EXPECTED_SIMPLE_FILES = ['file_1',
                    'file_2',
                    'dir_1/file1',
                    'dir_1/file2',
                    'dir_2/file1',
                    'dir_2/file2']

def run_tcf_backup(name: str):
    tcf_backup_dir = os.path.dirname(TCF_BACKUP)
    sp.check_call(['systemd-nspawn',
                   '-D', '/',
                   '--ephemeral',
                   '--user=root',
                   f'--bind-ro={tcf_backup_dir}:/usr/local/bin',
                   f'--bind-ro={CONFIG_DIR}:/etc/tcf-backup',
                   f'--bind=/tmp/integration_tests',
                   'tcf-backup', 'backup', name])

def mktree(root: Path, files: list[str]):
    for file in files:
        p = Path(root / file)
        p.parent.mkdir(parents=True, exist_ok=True)
        p.touch()

def run_test(src: Path, dst: Path, dst_file: str, cfg: str, files: list[str], expected_files: list[str], create_dirs=True):
    if create_dirs:
        src.mkdir(parents=True, exist_ok=True)
        dst.mkdir(parents=True, exist_ok=True)

    try:
        mktree(src, files)
        run_tcf_backup(cfg)

        expected_files = set(expected_files)
        result = set((x[2:] if x.startswith('./') else x for x in sp.check_output(['tar', 'tf', dst / dst_file], encoding='utf-8').splitlines()))
        
        print('======')
        if len(diff:= expected_files.difference(result)) > 0:
            print('Difference in files:')
            
            print('Source:')
            for f in sorted(expected_files):
                print(f'\t{f}')

            print('Result:')
            for f in sorted(result):
                print(f'\t{f}')
        else:
            print(f'{cfg}: OK')
        print('======')

    finally:
        if create_dirs:
            shutil.rmtree(src)
            shutil.rmtree(dst)


def dir2dir():
    src = Path('/tmp/integration_tests/dir2dir_source')
    dst = Path('/tmp/integration_tests/dir2dir_target')
    
    run_test(src, dst, 'dir2dir.tar.gz', 'dir2dir', SIMPLE_FILES, EXPECTED_SIMPLE_FILES)

def btrfs2dir():
    src = Path('/tmp/integration_tests/btrfs2dir_source')
    dst = Path('/tmp/integration_tests/btrfs2dir_target')
    subvolumes = src / 'subvolumes'
    root = subvolumes / 'root'
    snapshot = subvolumes / 'snapshots'
    btrfs_disk = src / 'btrfs_drive'

    try:
        src.mkdir(parents=True)
        dst.mkdir(parents=True)
        subvolumes.mkdir(parents=True)

        sp.check_call(['fallocate', '-l', '64M', btrfs_disk])
        sp.check_call(['mkfs.btrfs', '-M', btrfs_disk])
        sp.check_call(['mount', btrfs_disk, subvolumes])
        sp.check_call(['btrfs', 'subvolume', 'create', root])
        sp.check_call(['btrfs', 'subvolume', 'create', snapshot])

        run_test(root, dst, 'btrfs2dir.tar.gz', 'btrfs2dir', SIMPLE_FILES, EXPECTED_SIMPLE_FILES, create_dirs=False)

    finally:
        sp.check_call(['umount', subvolumes])
        btrfs_disk.unlink()
        shutil.rmtree(src)
        shutil.rmtree(dst)

dir2dir()
btrfs2dir()