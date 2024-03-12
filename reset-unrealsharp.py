import sys
import os
import argparse
import shutil

def _remove_directory(directory_path) :
    if(os.path.exists(directory_path)) :
        shutil.rmtree(directory_path)
        print("delete directory:" + directory_path)

script_directory_path = os.path.dirname(os.path.realpath(__file__))

_delete_target_directories = [
    "Content/CSharpBlueprints", # import assets path
    "Intermediate/UnrealSharp"  # temp files    
]

_recusive_delete_directory_names = [
    "UnrealSharpBinding", # C++ generated files
    "Bindings", # api bindings
    "Bindings.Placeholders", # place holder files
    "obj", # C# tmep directories
]

def _delete_common_directories() :
    for p in _delete_target_directories :
        dir = os.path.join(script_directory_path, p)
        if(os.path.exists(dir)) :
            _remove_directory(dir)

def _recusive_delete_directories(root_directory):
    for dirpath, dirnames, _ in os.walk(root_directory, topdown=False):
        for dirname in dirnames:
            if dirname in _recusive_delete_directory_names:
                full_dir_path = os.path.join(dirpath, dirname)
                _remove_directory(full_dir_path)

_delete_common_directories()

for root in [
    "GameScripts",
    "Source"
] :
    _recusive_delete_directories(root)


