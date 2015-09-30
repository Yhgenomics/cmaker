#!/bin/sh

echo "You must install mono-complete"
echo "Start installing..."
mcs Program.cs
cp Program.exe /usr/bin/CMaker
echo "Install finish"