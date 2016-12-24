clc
a= imread('F:\stego matlab code\test.jpg');
d=zeros(size(a,1),size(a,2),size(a,3));
% disp(a);
% size(d);
  
  title('Before processin');
  
  imshow(a);
  figure;
  title('Before processin histo');
  
  b=rgb2gray(a);
  imhist(b);
  
  for  l= 1:size(a,3)
    for i = 1:size(a,1)
        for j = 1:size(a,2)
            pix=a(i,j,l);
%             disp(pix)
            for k = 8:-1:1
                b = bitget(pix,k);
%                 disp(b);
                 if(b==1)
                
                    if(k==6 || k==7 || k==8)
                        pix = bitand(pix,248);
                        pix = bitor(pix,round(rand(1)*7));
                    end
                    if(k==4 || k==3 || k==5)
                        pix = bitand(pix,252);
                        pix = bitor(pix,round(rand(1)*3));
                    end
                    if(k==2 || k==1)
                        pix = bitand(pix,254);
                        pix = bitor(pix,round(rand(1)*1));
                    end
%                     d(i,j,l)=pix;
                    a(i,j,l)=pix;
%                     disp(k);
                    
                    break;
                 end
            end
        end
    end
  end
%     disp(a);
% disp(d);
  figure;
  title('After processin');
  
  imshow(a);
  figure;
  title('After processin histo');
  b=rgb2gray(a);
  
  imhist(b);