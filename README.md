# MNIST Builder for PyTorch

<img src="assets/images/app.png" width="800"/>

Windows tool for generating MNIST-style character datasets from font files. It works specifically with the Google Fonts repository structure, so the Google Fonts Git repository is required as input.

The application loads fonts, renders glyphs across digits, lowercase, and uppercase sets, normalizes them into consistent **21×21 pixel** grayscale images, and exports them into a folder-based labeling format compatible with PyTorch-style datasets. The workflow is focused on deterministic, repeatable dataset generation rather than training or experimentation.  

## User Guide
### Prerequisite & Setup:
- Download the precompiled binary (recommended): https://github.com/mgoyalitm/mnist-builder-for-pytorch/releases
- OR clone this repository and run/debug in your preferred IDE (Visual Studio is recommended for WPF)  
- Download or clone the Google Fonts repository (required input): https://github.com/google/fonts 
### First Launch
- On first launch, you need to point the application to the Google Fonts repository root directory  
- Press `Ctrl + O` to browse and select the Google Fonts folder  
- Once selected, the application will start loading fonts and display a loading screen  
- During this phase, fonts are validated and processed  
- This may take some time depending on your system performance and the size of the repository  

<table align="center">
  <tr>
    <td align="center">
      <img src="assets/images/first-launch.png" width="100%"/><br/>
      <b>First Launch</b>
    </td>
    <td width="10"></td>
    <td align="center">
      <img src="assets/images/loading.png" width="100%"/><br/>
      <b>Loading Screen</b>
    </td>
  </tr>
</table>

### Navigation & Font Selection:
- After loading the Google Fonts repository, the main screen is displayed  
- The center panel shows the currently selected font  
- The top title displays font metadata:
    - Font name  
    - Style (Regular, Italic)  
    - Category (Sans Serif, Serif, etc.)  
    - Weight  (Normal, Bold, etc.)
- A preview section renders:
    - "the quick brown fox jumps over the lazy dog"  
    - Lowercase, uppercase, and digits (0–9)  
- A montage view shows the same glyphs with applied rotations  
- Use `←` and `→` arrow keys to navigate between fonts  
- Press `Enter` to add the currently previewed font to the bucket  
- Added fonts appear in the right-side panel
- Click a font in the bucket to load it back into the preview screen

<p align="center">
  <img src="assets/images/components.png" width="70%" alt="Preview and Font Bucket"/>
  <br/>
  <b>Preview & Font Bucket</b>
</p>

### Delete from Bucket & Clear Bucket:
- Press `Delete` to remove the currently previewed font from the bucket (useful right after adding with `Enter`)  
- To remove a specific font after navigating, right-click the font in the bucket and press `Delete`  
- Press `Shift + Delete` to clear the entire bucket  
    - A confirmation dialog will appear:
    - `Enter` → Confirm  
    - `Esc` → Cancel  
    - Or click `Yes` / `No`  
- Pro Tip:
  - When reviewing previously added fonts, click a font in the bucket to load it in the preview  
  - Visually verify if the font is unsuitable before deleting  
  - Press `Delete` to remove it if needed  

### Filter, Bulk Import & Bulk Remove:
- Press `Ctrl + F` to open the Filter dialog  
    - Select Categories, Styles, and Weights (multiple selections allowed)  

- Filtering logic:
    - Within the same column (Category / Style / Weight) → **OR** logic  
    - Across columns → **AND** logic  

- Once criteria is set:
    - Press `Enter` or click `Import` to add all matching fonts to the bucket  
    - Click `Remove` or press `Delete` to remove all matching fonts from the bucket  
    - Enable `Filter Results` to apply the filter directly to font navigation on the main screen  

<p align="center">
  <img src="assets/images/filter-window.png" width="70%" alt="Preview and Font Bucket"/>
  <br/>
  <b>Filter Dialog</b>
</p>

### Generate MNIST:
  - Press `Ctrl + S` to select the output directory where MNIST data will be generated  
  - Press `F5` to generate **21×21 pixel** images for digits, lowercase, and uppercase characters  
  - A progress bar will appear between the preview panel and the bottom hints section during generation  
  - If the output directory is not set, a folder selection dialog will appear before generation starts  
  - Caution:
    - Before MNIST generation, all existing files and folders inside the selected output directory will be deleted.
<br/>
<br/>
<p align="center">
  <img src="assets/images/generate-mnist.png" width="70%" alt="MNIST generation progress"/>
  <br/>
  <b>Progress of MNIST dataset generation shown on progress bar</b>
</p>

#### MNIST Output Structure
    output/
    ├── digit_0/
    ├── digit_1/
    ├── digit_2/
    ├── digit_3/
    ├── ...
    ├── lower_a/
    ├── lower_b/
    ├── lower_c/
    ├── ...
    ├── upper_A/
    ├── upper_B/
    ├── upper_C/
    ├── ...

### Closing & Relaunch:
- On closing the application, the current state is saved automatically  
- On next launch, the application restores the previous session:
  - Previously selected Google Fonts repository  
  - Bucket contents  
  - Applied filters  
  - Last previewed font  
- This allows you to continue from where you left off without reconfiguration  
- State restoration works only if the application is closed properly  

## Keyboard Shortcuts

#### Application Main Window

| **Key** | Action |
|-----|--------|
| Ctrl + O | Open and select the Google Fonts repository root directory |
| Ctrl + S | Set the output directory for MNIST dataset generation |
| F5 | Generate and write the MNIST dataset (21×21 images) to disk |
| ← / → | Navigate to previous / next font |
| Enter | Add the currently previewed font to the bucket |
| Delete | Remove the currently previewed font from the bucket |
| Shift + Delete | Clear all fonts from the bucket (with confirmation) |
| Ctrl + F | Open the filter dialog |

#### Filter Dialog
| Key | Action |
|-----|--------|
| Enter | Import all fonts matching the selected filter criteria into the bucket |
| Delete | Remove all fonts from the bucket that match the current filter criteria |
| Esc | Close the filter dialog |

#### Confirm Dialog
| Key | Action |
|-----|--------|
| Enter | Confirm action (equivalent to clicking **Yes**) |
| Esc | Cancel action (equivalent to clicking **No**) |
