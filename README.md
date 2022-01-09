# MLAgent-Assignment
```
Make sure to run this project in Unity Version: 2020.3.25f1
```
## Scene Linking
You have to put all the scenes into build settings in order to perform scene switching in unity editor game play.

## Troubleshooting
Possible solutions of problem you may encounter:
1. Window > Package Manager > Packages: Unity Registry > Install 'Input System' & 'Post Processing'

2. In Hierarchy:
Post Processing > Post Process Volume > tick 'Is Global' > SELECT 'PP_Profile_Weapon Post Process Profile...' in Profile

3. Go to files>> build settings >> player settings>> player >> go down and change active input handling to ‘both’