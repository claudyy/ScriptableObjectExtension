# ScriptableObjectExtension
With scriptable object Extension it is easier to work with unity's scriptableobject. Unity has a functionality where you can assign scriptable object to other scriptable object. But these is not easy to do, you have to implement this yourself. With this extension it is super easy to do this. When having a lot of scriptable objects it can be hard to balance all the values. So the export features can make this better. You can export your data and put them into spreadsheet, where you can have all your at one glance.

# SubAsset
SubAsset makes it easier to create child scriptable objects. It gives you an easy way to create childs. And some additional functionality. You can sort, delete or copy them. You have to inherent from ScriptableObject Parent. Now you can add childs in the editor. But you can override some function to custom it more to your need.

Folder() With this you can define where the pop up window that adds the child start looking at.

GetIndent(ScriptableObject subAsset) If you want you sub Assets to have an indent in the editor you can return here an indent number. by default it is zero.

GetWarning(ScriptableObject subAsset) you can color the sub Asset red if something is wrong with it. Return true to color it red.

GetWarningMessage(ScriptableObject subAsset) If GetWarning returns true you can also implement a message that should be shown.

GetAssets() This function return a list of all sub assets of the type T.

# ConditionStack
The Condition stack inherent from ScriptalbeObjectParent, but give additional feature. You can add condition and effects as child assets. You can call GetEffects on the ConditionStack and get all effects for which the conditions are meet.

# Exporter
With the exporter you can export scriptableObjects and you have full control of what values get exported.
Credit: This exporter is heavily inspired by work from Martin Nerurkar. You can find some interesting scripting concepts on the blog of his company: http://www.sharkbombs.com/blog/

# Reflection Exporter
The reflection exporter is an easy way to get all your scriptableObjects values into a spreadsheet. But you have no control of the what values get exported. You can open the GenericReflactionExporter to export all ScriptableObject. But if you want to limit what you Export you can inherent from BaseReflectionExporter and limit what he exports and change how the the exporter create the values for the types.
 
# To do
ConditionStack:
Example for ConditionStack to test if all the features are there and functional
Reflection Exporter 
the BaseReflectionExporter should take care of types. He should be able to find all object the type in the project and assign them. Like sprites,ScriptableObject, Mesh and Prefabs.


