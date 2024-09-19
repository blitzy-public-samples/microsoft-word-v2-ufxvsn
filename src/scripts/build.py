import os
import subprocess
import argparse
import shutil
import logging

# Define global variables
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.abspath(os.path.join(SCRIPT_DIR, '..', '..'))
BUILD_DIR = os.path.join(ROOT_DIR, 'build')
SRC_DIR = os.path.join(ROOT_DIR, 'src')
logger = logging.getLogger(__name__)

def setup_logging():
    """Sets up logging configuration"""
    logging.basicConfig(format='%(asctime)s - %(name)s - %(levelname)s - %(message)s', level=logging.INFO)

def parse_arguments():
    """Parses command-line arguments"""
    parser = argparse.ArgumentParser(description="Build Microsoft Word web application")
    parser.add_argument('--build-type', choices=['debug', 'release'], default='release', help="Build type (debug/release)")
    parser.add_argument('--target', choices=['web', 'windows', 'macos'], required=True, help="Target platform (web/windows/macos)")
    return parser.parse_args()

def clean_build_directory():
    """Cleans the build directory"""
    if os.path.exists(BUILD_DIR):
        shutil.rmtree(BUILD_DIR)
    os.makedirs(BUILD_DIR)

def build_web(build_type):
    """Builds the web version of the application"""
    os.chdir(os.path.join(SRC_DIR, 'web'))
    
    # Install dependencies
    subprocess.run(['npm', 'install'], check=True)
    
    # Build the application
    build_command = ['npm', 'run', 'build']
    if build_type == 'debug':
        build_command.append('--debug')
    subprocess.run(build_command, check=True)
    
    # Copy build output to BUILD_DIR
    shutil.copytree('build', os.path.join(BUILD_DIR, 'web'))

def build_windows(build_type):
    """Builds the Windows version of the application"""
    os.chdir(os.path.join(SRC_DIR, 'windows'))
    
    # Restore dependencies
    subprocess.run(['dotnet', 'restore'], check=True)
    
    # Build the application
    build_command = ['dotnet', 'build']
    if build_type == 'release':
        build_command.extend(['-c', 'Release'])
    subprocess.run(build_command, check=True)
    
    # Publish the application
    publish_command = ['dotnet', 'publish', '-o', os.path.join(BUILD_DIR, 'windows')]
    if build_type == 'release':
        publish_command.extend(['-c', 'Release'])
    subprocess.run(publish_command, check=True)

def build_macos(build_type):
    """Builds the macOS version of the application"""
    os.chdir(os.path.join(SRC_DIR, 'macos'))
    
    # Build the application using xcodebuild
    build_command = ['xcodebuild']
    if build_type == 'release':
        build_command.extend(['-configuration', 'Release'])
    else:
        build_command.extend(['-configuration', 'Debug'])
    subprocess.run(build_command, check=True)
    
    # Copy build output to BUILD_DIR
    build_output = 'build/Release' if build_type == 'release' else 'build/Debug'
    shutil.copytree(build_output, os.path.join(BUILD_DIR, 'macos'))

def main():
    """Main function to orchestrate the build process"""
    setup_logging()
    args = parse_arguments()
    
    logger.info(f"Starting build process for {args.target} platform in {args.build_type} mode")
    
    clean_build_directory()
    
    if args.target == 'web':
        build_web(args.build_type)
    elif args.target == 'windows':
        build_windows(args.build_type)
    elif args.target == 'macos':
        build_macos(args.build_type)
    
    logger.info(f"Build completed successfully for {args.target} platform")

if __name__ == '__main__':
    main()