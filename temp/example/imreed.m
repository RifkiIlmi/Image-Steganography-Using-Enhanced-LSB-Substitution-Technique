function [x,y,z] = imreed(a)
c = imread(a); 
x=c(:,:,1);
y=c(:,:,2);
z=c(:,:,3);
end