# Changelog

## [0.12.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.11.0...v0.12.0) (2026-06-14)


### Features

* remove hooks/ scaffold from add-feature and add remove-feature subdir test ([#69](https://github.com/piers-sinclair/Dostar.Cli/issues/69)) ([3321165](https://github.com/piers-sinclair/Dostar.Cli/commit/3321165cc259582ccbb7373eb24c9a9f59da7f90))

## [0.11.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.10.0...v0.11.0) (2026-06-14)


### Features

* scaffold component file from add-feature and protect bare dostar in JSX substitution ([#68](https://github.com/piers-sinclair/Dostar.Cli/issues/68)) ([f05760a](https://github.com/piers-sinclair/Dostar.Cli/commit/f05760a38702bdf26aba0f9e457d04d2c5a1db90))


### Documentation

* document add-feature and remove-feature in README ([#66](https://github.com/piers-sinclair/Dostar.Cli/issues/66)) ([2feed59](https://github.com/piers-sinclair/Dostar.Cli/commit/2feed59fd43ff88171aa069e1914ca4befcb2c2c))

## [0.10.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.9.0...v0.10.0) (2026-06-14)


### Features

* **add-feature:** remove --type option and simplify to single scaffold shape ([#64](https://github.com/piers-sinclair/Dostar.Cli/issues/64)) ([76842da](https://github.com/piers-sinclair/Dostar.Cli/commit/76842da379ea0bfadf9f4c5e2bbcdc6601fb87bb))


### Bug Fixes

* **token-replace:** protect dostar add-feature, remove-feature, and dostar:feature: sentinel prefix from substitution ([#62](https://github.com/piers-sinclair/Dostar.Cli/issues/62)) ([672e58b](https://github.com/piers-sinclair/Dostar.Cli/commit/672e58b783fb5065f68ec117290605053fa59c46)), closes [#61](https://github.com/piers-sinclair/Dostar.Cli/issues/61)

## [0.9.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.8.0...v0.9.0) (2026-06-14)


### Features

* **cli:** generate per-feature route files and wire nav links into __root.tsx ([#60](https://github.com/piers-sinclair/Dostar.Cli/issues/60)) ([4653e05](https://github.com/piers-sinclair/Dostar.Cli/commit/4653e05dfe5dcd5ddd11d739dbe74157740ffab7)), closes [#57](https://github.com/piers-sinclair/Dostar.Cli/issues/57)


### Bug Fixes

* **template:** replace _values with values in FeatureForm to fix ESLint no-unused-vars error ([#58](https://github.com/piers-sinclair/Dostar.Cli/issues/58)) ([20ab4f9](https://github.com/piers-sinclair/Dostar.Cli/commit/20ab4f9a2308beb552c521a6f6fcf410313ac140)), closes [#56](https://github.com/piers-sinclair/Dostar.Cli/issues/56)

## [0.8.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.7.0...v0.8.0) (2026-06-14)


### Features

* **cli:** add --type option to add-feature for list, form, and none shapes ([#55](https://github.com/piers-sinclair/Dostar.Cli/issues/55)) ([56c1020](https://github.com/piers-sinclair/Dostar.Cli/commit/56c102049a1a427a124c9918570478030b3530cc))
* **cli:** scaffold List component and wire routes/index.tsx with sentinels in add-feature ([#53](https://github.com/piers-sinclair/Dostar.Cli/issues/53)) ([306850f](https://github.com/piers-sinclair/Dostar.Cli/commit/306850f331b200f2a25d3a5697362448b26ecfb7))

## [0.7.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.6.0...v0.7.0) (2026-06-13)


### Features

* detect gh CLI and tailor new-project next-steps instructions ([#40](https://github.com/piers-sinclair/Dostar.Cli/issues/40)) ([f71c4b5](https://github.com/piers-sinclair/Dostar.Cli/commit/f71c4b5657f7c24197bb8dd2f7d734f5a90f7247))
* replace CLAUDE.md template intro paragraph in new-project ([#48](https://github.com/piers-sinclair/Dostar.Cli/issues/48)) ([3591a2f](https://github.com/piers-sinclair/Dostar.Cli/commit/3591a2f0fcec6d90b5cbafe079884f30e6281dd7))
* strip README.md template content in new-project ([#47](https://github.com/piers-sinclair/Dostar.Cli/issues/47)) ([93731de](https://github.com/piers-sinclair/Dostar.Cli/commit/93731deb6e4f90463fd27c413fb3e7235f111ad8))

## [0.6.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.5.0...v0.6.0) (2026-06-13)


### Features

* minimal scaffold wiring + Claude skill hints ([#38](https://github.com/piers-sinclair/Dostar.Cli/issues/38)) ([8f464bf](https://github.com/piers-sinclair/Dostar.Cli/commit/8f464bf4029d6635644cb9f561a78309545fb01e))

## [0.5.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.4.0...v0.5.0) (2026-06-13)


### Features

* include git remote hint in post-scaffold next steps ([#35](https://github.com/piers-sinclair/Dostar.Cli/issues/35)) ([b992933](https://github.com/piers-sinclair/Dostar.Cli/commit/b9929338f9373075253a4c4f3bbf4d02e432e8dd))
* initialise git repo after scaffolding new project ([#34](https://github.com/piers-sinclair/Dostar.Cli/issues/34)) ([29ad9de](https://github.com/piers-sinclair/Dostar.Cli/commit/29ad9de843fbd05db393cf9bd46ff2cfbcd9a0cf))

## [0.4.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.3.0...v0.4.0) (2026-06-12)


### Features

* add [@no-substitute](https://github.com/no-substitute) line annotation to ProjectNameSubstitutor ([#32](https://github.com/piers-sinclair/Dostar.Cli/issues/32)) ([3674612](https://github.com/piers-sinclair/Dostar.Cli/commit/36746124aa7d8e18df7a44ea0aae0f0036cfd924))

## [0.3.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.2.1...v0.3.0) (2026-06-12)


### Features

* add add-feature command to scaffold frontend feature folder ([#31](https://github.com/piers-sinclair/Dostar.Cli/issues/31)) ([31d87da](https://github.com/piers-sinclair/Dostar.Cli/commit/31d87da4bf9c677406058da47c5bd891f057f4ab))
* add remove-feature command to delete frontend feature folder ([#30](https://github.com/piers-sinclair/Dostar.Cli/issues/30)) ([a1c6b48](https://github.com/piers-sinclair/Dostar.Cli/commit/a1c6b4887d336c3fd8131f213e5a9f65d1431d87)), closes [#29](https://github.com/piers-sinclair/Dostar.Cli/issues/29)
* remove frontend feature folder when removing a module ([#27](https://github.com/piers-sinclair/Dostar.Cli/issues/27)) ([b91c0a7](https://github.com/piers-sinclair/Dostar.Cli/commit/b91c0a7f3dddf0cb94a43c130ef578cdc12d84e6))


### Bug Fixes

* **ci:** make path-filtered CI checks always report to satisfy required checks ([#22](https://github.com/piers-sinclair/Dostar.Cli/issues/22)) ([3b0e760](https://github.com/piers-sinclair/Dostar.Cli/commit/3b0e760e0b9cc66cdcb2540a3706b67639f9a125)), closes [#16](https://github.com/piers-sinclair/Dostar.Cli/issues/16)
* reset CHANGELOG.md and version on new-project ([#23](https://github.com/piers-sinclair/Dostar.Cli/issues/23)) ([98da839](https://github.com/piers-sinclair/Dostar.Cli/commit/98da8390a2bb759f9b9ed601105f2ef51bef9fe3)), closes [#17](https://github.com/piers-sinclair/Dostar.Cli/issues/17)
* strip cross-repo dependency section from CLAUDE.md on new-project ([#24](https://github.com/piers-sinclair/Dostar.Cli/issues/24)) ([b052153](https://github.com/piers-sinclair/Dostar.Cli/commit/b0521538f4ad65b745452bb25f9885439784e34c))
* **templates:** align add-module output with current Todos module conventions ([#16](https://github.com/piers-sinclair/Dostar.Cli/issues/16)) ([f2b8f30](https://github.com/piers-sinclair/Dostar.Cli/commit/f2b8f300eba244442cda4106baa2d1401eb1f408))


### Documentation

* clarify Dostar's three core goals in README and CLAUDE.md ([#20](https://github.com/piers-sinclair/Dostar.Cli/issues/20)) ([a46e12a](https://github.com/piers-sinclair/Dostar.Cli/commit/a46e12a129b3d80150d5198b02ee1c2825aec2c2))

## [0.2.1](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.2.0...v0.2.1) (2026-06-11)


### Bug Fixes

* preserve CLI tool references during project-name substitution ([#15](https://github.com/piers-sinclair/Dostar.Cli/issues/15)) ([0e8741e](https://github.com/piers-sinclair/Dostar.Cli/commit/0e8741e67190dfd622e63b71c02291ca69b69999))
* replace post-scaffold commands with devcontainer instructions ([#12](https://github.com/piers-sinclair/Dostar.Cli/issues/12)) ([7888178](https://github.com/piers-sinclair/Dostar.Cli/commit/788817873853bd01272544ef612687af30bf2559))

## [0.2.0](https://github.com/piers-sinclair/Dostar.Cli/compare/v0.1.0...v0.2.0) (2026-06-11)


### Features

* add --no-endpoints flag to add-module ([7b22629](https://github.com/piers-sinclair/Dostar.Cli/commit/7b22629bb8aea9be948ece3e70e2e514f9d3dd86))
* add test suite for add-module and remove-module ([0ffc072](https://github.com/piers-sinclair/Dostar.Cli/commit/0ffc072ba00e3df0ef2669ca0b3cea4fa675306f))
* CLI dostar add-module command ([b98d2f3](https://github.com/piers-sinclair/Dostar.Cli/commit/b98d2f35ee4d8f8325fac4415bfeeadac02cddb7))
* CLI dostar add-module command ([4f09f27](https://github.com/piers-sinclair/Dostar.Cli/commit/4f09f27be168441d44b09829f178596bcf49a364)), closes [#1](https://github.com/piers-sinclair/Dostar.Cli/issues/1)
* CLI dostar remove-module subcommand ([3eb9516](https://github.com/piers-sinclair/Dostar.Cli/commit/3eb95167a473db811df7fada062f22a7b20791ae))
* CLI dostar remove-module subcommand ([a6f6ea4](https://github.com/piers-sinclair/Dostar.Cli/commit/a6f6ea4beb74a46241c8caba6a4d8734f22a7418)), closes [#2](https://github.com/piers-sinclair/Dostar.Cli/issues/2)
* initial Dostar CLI source ([01cd764](https://github.com/piers-sinclair/Dostar.Cli/commit/01cd76476d5366968de86d55c323727b7317fd05))
* **new-project:** add --owner option to replace __GITHUB_ORG__ placeholder URLs ([dd8265f](https://github.com/piers-sinclair/Dostar.Cli/commit/dd8265f679cf421a086733815be2bc00b91a706b))
* **new-project:** add --owner option to stamp GitHub org into project URLs ([a8f2148](https://github.com/piers-sinclair/Dostar.Cli/commit/a8f2148029da2645f9859bcbc0c9e266d833ab3b))


### Bug Fixes

* derive project prefix from .slnx filename — support any project name ([7b1c4e2](https://github.com/piers-sinclair/Dostar.Cli/commit/7b1c4e22b004b3388308b4f11aceb4b1890eb8f0))
* register new-project command in Program.cs ([7e7e1b3](https://github.com/piers-sinclair/Dostar.Cli/commit/7e7e1b3a56b1a9310d3f0e07e6ff61e585108464))
* stamp generated LICENSE with caller's author name and current year ([#10](https://github.com/piers-sinclair/Dostar.Cli/issues/10)) ([12c941d](https://github.com/piers-sinclair/Dostar.Cli/commit/12c941d1610d376ed22c988efa98623d20a05ea5))
* upgrade Scriban 6.5.8 → 7.0.6 to resolve known vulnerabilities ([8841b68](https://github.com/piers-sinclair/Dostar.Cli/commit/8841b68b2937695085369e7933b4f27f71c54b86))


### Documentation

* add branch and PR workflow to CLAUDE.md ([c908b0c](https://github.com/piers-sinclair/Dostar.Cli/commit/c908b0c011b542dd64ee12b0f6dffc0b079f208e))
* add CLAUDE.md with project context for Claude Code ([22e09e6](https://github.com/piers-sinclair/Dostar.Cli/commit/22e09e641cc4addb0badf588da1211447f4cc781))
* add cross-repo dependency guidance to CLAUDE.md ([d0b83a7](https://github.com/piers-sinclair/Dostar.Cli/commit/d0b83a72af57e77cbb2d049b1405db6a0889a8bb))
* fix README for standalone repo ([5e1b337](https://github.com/piers-sinclair/Dostar.Cli/commit/5e1b3376df6fc57a31ea14c58c27d58615df876d))

## Changelog

All notable changes to this project will be documented in this file. See [release-please](https://github.com/googleapis/release-please) for commit guidelines.
