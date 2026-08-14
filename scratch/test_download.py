import urllib.request
import urllib.parse
import os

def main():
    latex = r"\dpi{300}\bg{white}X = \begin{bmatrix} y_1 & y_2 & \cdots & y_K \\ y_2 & y_3 & \cdots & y_{K+1} \\ \vdots & \vdots & \ddots & \vdots \\ y_L & y_{L+1} & \cdots & y_N \end{bmatrix}"
    url = "https://latex.codecogs.com/png.image?" + urllib.parse.quote(latex)
    try:
        req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
        with urllib.request.urlopen(req) as response:
            with open("test_eq.png", "wb") as f:
                f.write(response.read())
        print("Test passed! Equation downloaded successfully.")
    except Exception as e:
        print("Failed to download:", e)

if __name__ == "__main__":
    main()
