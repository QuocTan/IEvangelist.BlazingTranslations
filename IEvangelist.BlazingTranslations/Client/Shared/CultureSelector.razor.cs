﻿using IEvangelist.BlazingTranslations.Client.Interop;
using IEvangelist.BlazingTranslations.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace IEvangelist.BlazingTranslations.Client.Shared
{
    public partial class CultureSelector
    {
        [Inject] IStringLocalizer<CultureSelector> Localizer { get; set; }
        [Inject] IJSRuntime JavaScript { get; set; }
        [Inject] NavigationManager Navigation { get; set; }
        [Inject] HttpClient Http { get; set; }

        IEnumerable<CultureInfo> _supportedCultures;
        CultureInfo _selectedCulture;

        CultureInfo Culture
        {
            get
            {
                if (CultureInfo.CurrentCulture is null)
                {
                    if (JavaScript is IJSInProcessRuntime jsInProcessRuntime)
                    {
                        var browserCulture = jsInProcessRuntime.GetCulture();
                        if (!string.IsNullOrWhiteSpace(browserCulture))
                        {
                            CultureInfo.CurrentCulture = new CultureInfo(browserCulture);
                        }
                    }
                }

                return CultureInfo.CurrentCulture;
            }
            set
            {
                if (value != null &&
                    CultureInfo.CurrentCulture != value)
                {
                    CultureInfo.CurrentCulture = value;
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            var azureCultures =
                await Http.GetFromJsonAsync<AzureCultures>(
                    "api/cultures",
                    DefaultOptions.SerializerOptions);

            _supportedCultures =
                CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Where(culture =>
                    azureCultures.Translation.ContainsKey(culture.TwoLetterISOLanguageName));
        }

        protected async Task SetCultureAsync()
        {
            Culture = _selectedCulture;
            if (JavaScript is IJSInProcessRuntime jsInProcessRuntime)
            {
                await jsInProcessRuntime.SetCultureAsync(Culture);
                Navigation.NavigateTo(Navigation.Uri, forceLoad: true);
            }
        }
    }
}