import re
import os
import subprocess
import sys
import shutil

# Gets commit messages starting from specified version until the next version (if presented)
# Creates version change lists files

# lists
ChangedList = []
FixedList = []
PolishingList = []
AddedList = []
RemovedList = []

ControlEntries = [    
    ("+", "Added", AddedList),  # each list consists of MessageEntry [message,tagsList]
    ("*", "Changed", ChangedList), 
    ("~", "Polished", PolishingList),
    ("!", "Fix", FixedList), 
    ("-", "Removed", RemovedList)
]
    
tags = ('#code', '#gfx', '#gui', '#design', '#content', '#art', '#misc') # sorted by priority



def GetControlEntry( symbol ):
    for entry in ControlEntries:
        if( entry[0] == symbol):
            return entry
    return None

def ParseTags( line ):
    splited = line.split()
    nospaced = "".join(splited)
    tagsList = nospaced.split("#")
    if len(tagsList) > 1:
        tagsList = tagsList[1:]
        
        #check tags
        for tagName in tagsList:
            if not tagName in str(tags):
                print( ">>> Error in tags. '{0}' is not registered tag name. But it still will be added".format(tagName))

        return tagsList


def ParseChanges( comment ):
    # clean comment and get rid of non-format comments
    cleanComment = []
    
    for line in comment.splitlines():
        if not line:
            continue

        line = line.strip()
                    
        if line.startswith('***'):
            continue
            
           
        print(">>>" + line)

        controlSymbol = line[0]
        
        if GetControlEntry(controlSymbol) == None:
            continue

        cleanComment.append( line )

    
    for line in cleanComment:
        # remove control symbol
        controlSymbol = line[0]
        line = line[1:]
        line = line.lstrip()

        # get entry where to add message
        controlEntry = GetControlEntry(controlSymbol) # comment-tags
            
        # parse tags
        lineTagsList = ParseTags(line)
        if lineTagsList != None:            
            # strip tags from the message
            for tag in lineTagsList:
                line = line.replace('#' + tag, '')
            line = line.strip()            
        else:
            lineTagsList = []

        # add to the group
        controlEntry[2].append([line, lineTagsList]) 
            
        


        
def GetColorFromTag(tags):
    if "art" in tags:
        return "aqua"
    if "code" in tags:
        return "green"
    if "design" in tags:
        return "grey"    
    if "content" in tags:
        return "lightblue"
    if "gfx" in tags:
        return "teal"
    if "gui" in tags:
        return "olive"
    if "misc" in tags:
        return "white"
    return "white"
#testComment = " * changed filesA \n -AAAA #art #test \n + added AAAA \n + fileAAAA"
#testComment2 = " - removed filesB \n -BBBB \n + BBBBBBB \n + fileBBBBB  \n zzzz"
#ParseChanges( testComment )
#ParseChanges( testComment2 )


print("USAGE: PrepareCL.py from_version to_version. \nto_version could be HEAD")
project_dir = "../.."
from_ver = sys.argv[1]
to_ver = sys.argv[2]
owd = os.getcwd()
os.chdir(project_dir)
cmd = 'git log --reverse {}..{} --no-merges --pretty=format:%B'.format(from_ver, to_ver)
output = subprocess.check_output(cmd).decode("utf-8")
ParseChanges(output)


# print CL
print("----- " + to_ver)
for block in ControlEntries:
    messages = block[2]
    if len(messages) == 0:
        continue
    print("--- " + block[1])
    for message in block[2]:
        print(message)
        
if(to_ver == "HEAD"):
    to_ver = sys.argv[3]
        
os.chdir(owd)
f = open("generated/"+to_ver+".txt", "w+")

print("<b>"+to_ver+"</b>\n",file=f)
for block in ControlEntries:
    caption = block[1]
    messagesEntries = block[2]
    
    print("<b>{0}</b>".format(caption), file=f) # header
    for entry in messagesEntries:
        message = entry[0]
        tags = entry[1]
        color = 'white'
        if len(tags) > 0:
            color = GetColorFromTag(tags[0])
        print("* <color={0}>{1}</color>".format(color,message), file=f)
    print("",file=f)
    


f.close()        




# copy
src_files = os.listdir("generated")
for file_name in src_files:
    full_file_name_src = os.path.join("generated", file_name)
    full_file_name_dst = os.path.join("edited", file_name)
    if os.path.isfile(full_file_name_src) and not os.path.exists(full_file_name_dst):
        shutil.copy(full_file_name_src, "edited") 