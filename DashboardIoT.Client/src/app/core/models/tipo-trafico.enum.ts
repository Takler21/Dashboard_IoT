export enum TipoTrafico {
  Normal = 0,
  Escaneo = 1,
  FuerzaBruta = 2,
  CommandAndControl = 3,
  Ddos = 4,
  Exploit = 5,
  AtaqueWeb = 6,
  Exfiltracion = 7,
  Otro = 8,
}

export const TipoTraficoLabels: Record<TipoTrafico, string> = {
  [TipoTrafico.Normal]: 'Normal',
  [TipoTrafico.Escaneo]: 'Escaneo',
  [TipoTrafico.FuerzaBruta]: 'Fuerza Bruta',
  [TipoTrafico.CommandAndControl]: 'Command & Control',
  [TipoTrafico.Ddos]: 'DDoS',
  [TipoTrafico.Exploit]: 'Exploit',
  [TipoTrafico.AtaqueWeb]: 'Ataque Web',
  [TipoTrafico.Exfiltracion]: 'Exfiltración',
  [TipoTrafico.Otro]: 'Otro',
};

export const TipoTraficoLabelsByName: Record<string, string> = {
  Normal: 'Normal',
  Escaneo: 'Escaneo',
  FuerzaBruta: 'Fuerza Bruta',
  CommandAndControl: 'Command & Control',
  Ddos: 'DDoS',
  Exploit: 'Exploit',
  AtaqueWeb: 'Ataque Web',
  Exfiltracion: 'Exfiltración',
  Otro: 'Otro',
};
