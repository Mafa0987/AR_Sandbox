import PyKinectV2
import PyKinectRuntime
import cv2
import numpy as np
import time
import PIL
from PIL import Image
from cvzone.HandTrackingModule import HandDetector
from PIL import Image
from matplotlib import cm
import os

def processDepthData(depthData):
    width = 512
    height = 424
    img = np.zeros((height, width, 3), dtype=np.float32)
    depthData_clipped = np.clip(depthData, 0, 1)
    img = GetColor(depthData_clipped)
    img = img[19:height-5, 124:width-88]
    return img

def GetColor(depthData):
    colors = np.array([[0, 0, 1], [0, 0.5, 1], [0, 1, 1], [0, 1, 0], [1, 0, 0]], dtype=np.float32)
    indices = np.clip((depthData * 4).astype(int), 0, 3)
    lowerBounds = indices / 4
    upperBounds = (indices + 1) / 4
    steps = upperBounds - lowerBounds
    colors1 = colors[indices]
    colors2 = colors[indices + 1]
    t = (depthData - lowerBounds) / steps
    interpolated_colors = Lerp(colors1, colors2, t)
    
    # Set colors to black where depth is exactly zero
    interpolated_colors[depthData == 0] = [0, 0, 0]
    
    return interpolated_colors

def Lerp(color1, color2, t):
    return (color2 - color1) * np.expand_dims(t, axis=-1) + color1

topLeftx = 627
topLefty = 5
bottomRightx = 1480
bottomRighty = 911

rate = 0
number = 1000
depthFrame = np.zeros((400, 300, 3), dtype=np.uint8)

def draw_circle(event,x,y,flags,param):
    print(x,y)

run = False

kinect = PyKinectRuntime.PyKinectRuntime(PyKinectV2.FrameSourceTypes_Depth | PyKinectV2.FrameSourceTypes_Color)
detector = HandDetector(maxHands=1, detectionCon=0.8)
while(True):
    if kinect.has_new_color_frame():
        depth = kinect.get_last_depth_frame()
        depth = depth.reshape((424,512))/4500
        depth = processDepthData(depth)
        color = kinect.get_last_color_frame()
        color = color.reshape((1080,1920,4))
        color = np.ascontiguousarray(color[:,:,:3], dtype=np.uint8)
        hand, color = detector.findHands(color)
        depth = cv2.flip(depth, 0)
        depth = np.uint8(depth * 255)
        depthCopy = depth.copy()
        if len(hand) > 0:
            points = np.array(hand[0]['lmList'])
            midx = (np.clip(int(points[0][0]), 0, 1920) + np.clip(int(points[5][0]), 0, 1920) + np.clip(int(points[17][0]), 0, 1920) + np.clip(points[1][0], 0, 1920))  // 4
            midy = (np.clip(int(points[0][1]), 0, 1080) + np.clip(int(points[5][1]), 0, 1080) + np.clip(int(points[17][1]), 0, 1080) + np.clip(points[1][1], 0, 1080)) // 4
            cv2.circle(color, (midx, midy), 15, (0, 255, 0), cv2.FILLED)
            depthx = int(np.clip((midx - topLeftx) / (bottomRightx - topLeftx) * 290, 0, 299))
            depthy = int(np.clip((midy - topLefty) / (bottomRighty - topLefty) * 320, 0, 399))
            depthy = 399 - depthy
            for i in range(depthy-5, depthy+5):
                for j in range(depthx-5, depthx+5):
                    if i >= 0 and i < 400 and j >= 0 and j < 300:
                        depthCopy[i, j] = [0, 255, 0]
            if run:
                if rate > 1:
                    rate = 0
                    if os.path.exists(f"C:/Users/mkf99/AR_Sandbox/NeuralNetwork/Data/PNG_A/Images/OpenHand/{number}.png"):
                        print("File already exists")
                        break
                    f = open(f"C:/Users/mkf99/AR_Sandbox/NeuralNetwork/Data/PNG_A/Positions/OpenHand/{number}.txt", "x")
                    f.write(str(depthx) + " " + str(depthy))
                    #f.write(str(0) + " " + str(0))
                    cv2.imwrite(f"C:/Users/mkf99/AR_Sandbox/NeuralNetwork/Data/PNG_A/Images/OpenHand/{number}.png", depth)
                    f.close()
                    if number == 1000+499:
                        break
                    else:
                        number += 1
                    print(number)
                else:
                    rate += 1
        cv2.imshow('image', depthCopy)
        if cv2.waitKey(1) & 0xFF == ord('r'):
            run = not run


