import numpy as np
import matplotlib.pyplot as plt
from sklearn.linear_model import LinearRegression
from sklearn.preprocessing import PolynomialFeatures
import pandas as pd

X = 4 * np.random.rand(100, 1) - 2
y = 4 + 2 * X + 5 * X**2 + 10 * X**3 + np.random.randn(100, 1)

poly_features = PolynomialFeatures(degree=(1,8), include_bias=False)
X_poly = poly_features.fit_transform(X)

reg = LinearRegression()
reg.fit(X_poly, y)

X_vals = np.linspace(-2, 2, 100).reshape(-1, 1)
X_vals_poly = poly_features.transform(X_vals)

y_vals = reg.predict(X_vals_poly)

plt.scatter(X, y)
plt.plot(X_vals, y_vals, color="r")
plt.show()
print(poly_features.degree)