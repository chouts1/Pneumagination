<tr>
<td>**Dual Specular Lobe Parametrization**</td>
<td>Specifies the method that controls the second specular lobe. The options are:<br/> &#8226; **Direct**: Gives you direct control of the second lobe. **SmoothnessB** controls the second lobe's smoothness, **LobeMix** controls the amount of mixing between the two lobes, and **Anisotropy** controls the second lobe's anisotropic smoothness.<br/> &#8226; **Hazy Gloss**: Adds a haze effect to the surface directly while keeping the primary lobe mostly unchanged. **Haziness** controls the intensity of the haze effect. **HazeExtent** controls the range of the haze around the central highlight. This option is the most intuitive and controllable way to use two lobes. Note that this mode implicitly modifies the specular color. If the material already has a high specular intensity (close to 1), there is no room to increase it in regions where haziness requires it to be boosted. In this case, haziness has little to no effect.<br/> &#8226; **From Diffusion Profile**: Use the **Dual Lobe Multipliers** and **Lobe Mix** settings in the Diffusion Profile.<br/>This setting only has an effect if you enable **Dual Specular Lobe**.</td>
</tr>