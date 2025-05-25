<a id="readme-top"></a>

[![Contributors][contributors-shield]][contributors-url] [![Forks][forks-shield]][forks-url] [![Stargazers][stars-shield]][stars-url] [![Issues][issues-shield]][issues-url] [![project_license][license-shield]][license-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://www.gitea.rulholos.fr/RulHolos/LunaForge">
    <img width="80px" height="80px" src="https://raw.githubusercontent.com/RulHolos/LunaForge/main/LunaForge/Images/Icon.png">
  </a>

<h3 align="center">LunaForge</h3>

  <p align="center">
    <strong>LunaForge</strong> is a code editor and generator targeting the LuaSTG engine (all main branches).
    <br />
    <a href="https://rulholos.github.io/LunaForge/editor/LunaForge.html"><strong>Explore the docs</strong></a>
    <br />
    <br />
    <a href="https://www.gitea.rulholos.fr/RulHolos/LunaForge/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://www.gitea.rulholos.fr/RulHolos/LunaForge/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#Differences with SharpX">Differences with Sharp X</a></li>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

The goal of LunaForge is to be a kind of "all-in-one" editor: It can compile plain lua script, compile nodes to lua, and shaders.<br />
There is more to come~

### Built With

[![.Net][dotnet]][NET-url] [![C#][csharp]][csharp-url] [![ImGUI][imgui-shield]][imgui-url] [![Raylib][raylib-shield]][raylib-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

Section in progress...

## Differences with Sharp X

The main "why" point with LunaForge is "[Sharp X](https://github.com/Sharp-X-Team/LuaSTG-Editor-Sharp-X) already exists, so why?"
- LunaForge uses a folder-based approach for projects instead of a single-file based approach.
- LunaForge uses ImGui instead of WPF.
- LunaForge allows you to set an entry point for your project, making it easier to structure your project.
- Projects are sharable by defaullt since all files are relative to the root of the project.
- LunaForge is meant to be cross-platform for Windows, Mac and Linux. (not the case yet)

### Prerequisites

Section in progress...

### Installation

Section in progress...

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

Section in progress...

_For more examples, please refer to the [Documentation](https://rulholos.github.io/LunaForge/editor/LunaForge.html)_

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap

- [x] Lua generated nodes
- [x] File Architecture
- [x] UI
- [x] UI customization
- [x] Node view
- [x] Script view
- [ ] Shader view
- [ ] Node cache
- [ ] Building projects
    - [x] Get Project files
    - [x] Gather compile info
    - [x] Check for checksum to avoid rebuilding the same file (if it exists in temp)
    - [ ] Create editor_output.lua and append every file
    - [ ] Run LuaSTG and link to debug info window
- [ ] Plugin system
- [ ] Logging

See the [open issues](https://www.gitea.rulholos.fr/RulHolos/LunaForge/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>

### Top contributors:

<a href="https://www.gitea.rulholos.fr/RulHolos/LunaForge/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=RulHolos/LunaForge" alt="contrib.rocks image" />
</a>



<!-- LICENSE -->
## License

All files from the **LunaForge.GUI.ImGuiFileDialog** namespace are taken and modified from [Dalamud](https://github.com/goatcorp/Dalamud).

Distributed under the MIT license. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Rül Hölos - [@RulHolos](https://twitter.com/RulHolos) - rulholos@protonmail.com

Project Link: [https://www.gitea.rulholos.fr/RulHolos/LunaForge](https://www.gitea.rulholos.fr/RulHolos/LunaForge)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [LuaSTG Editor Sharp X](https://github.com/Sharp-X-Team/LuaSTG-Editor-Sharp-X) (By Tom, Ryann, Zino and Rül)
* [LuaSTG branches](https://github.com/Legacy-LuaSTG-Engine)
* [LuaSTG English Wiki](https://luastgen.miraheze.org/wiki/Main_Page) (The miraheze one)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/RulHolos/LunaForge.svg?style=for-the-badge
[contributors-url]: https://www.gitea.rulholos.fr/RulHolos/LunaForge/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/RulHolos/LunaForge.svg?style=for-the-badge
[forks-url]: https://www.gitea.rulholos.fr/RulHolos/LunaForge/network/members
[stars-shield]: https://img.shields.io/github/stars/RulHolos/LunaForge.svg?style=for-the-badge
[stars-url]: https://www.gitea.rulholos.fr/RulHolos/LunaForge/stargazers
[issues-shield]: https://img.shields.io/github/issues/RulHolos/LunaForge.svg?style=for-the-badge
[issues-url]: https://www.gitea.rulholos.fr/RulHolos/LunaForge/issues
[license-shield]: https://img.shields.io/github/license/RulHolos/LunaForge.svg?style=for-the-badge
[license-url]: https://www.gitea.rulholos.fr/RulHolos/LunaForge/blob/master/LICENSE.txt
[product-screenshot]: images/screenshot.png
[dotnet]: https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white
[NET-url]: https://dotnet.microsoft.com/fr-fr/
[csharp]: https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=csharp&logoColor=white
[csharp-url]: https://dotnet.microsoft.com/fr-fr/languages/csharp
[imgui-shield]: https://img.shields.io/badge/ImGUI-20B2AA?style=for-the-badge
[imgui-url]: https://github.com/ocornut/imgui
[raylib-shield]: https://img.shields.io/badge/Raylib-AA5555?style=for-the-badge
[raylib-url]: https://www.raylib.com