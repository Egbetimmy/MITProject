import os
import matplotlib.pyplot as plt

def main():
    formula = r"X = \left[ \begin{array}{cccc} y_1 & y_2 & \cdots & y_K \\ y_2 & y_3 & \cdots & y_{K+1} \\ \vdots & \vdots & \ddots & \vdots \\ y_L & y_{L+1} & \cdots & y_N \end{array} \right]"
    fig, ax = plt.subplots(figsize=(6, 1.8))
    ax.text(0.5, 0.5, f"${formula}$", 
            horizontalalignment='center',
            verticalalignment='center',
            fontsize=15)
    ax.axis('off')
    plt.savefig("test_output.png")
    plt.close()
    print("Test passed successfully!")

if __name__ == "__main__":
    main()
